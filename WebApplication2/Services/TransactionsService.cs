using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;

namespace Trainacc.Services
{
    public class TransactionsService
    {
        private readonly AppDbContext _context;
        private readonly RestrictionsService _restrictionsService;
        public TransactionsService(AppDbContext context, RestrictionsService restrictionsService)
        {
            _context = context;
            _restrictionsService = restrictionsService;
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync()
        {
            return await _context.Transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction,
                IsAdd = t.IsAdd
            }).ToListAsync();
        }

        public async Task<TransactionDto?> GetTransactionAsync(int id)
        {
            var t = await _context.Transactions.FindAsync(id);
            if (t == null) return null;
            return new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction,
                IsAdd = t.IsAdd
            };
        }

        public async Task<TransactionDto> CreateTransactionAsync(TransactionCreateDto dto, int accountsId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountsId);
            if (account == null)
                throw new Exception("Счёт не найден");
            if (dto.TransactionValue < 0)
                throw new Exception("Сумма операции не может быть отрицательной");
            if (string.IsNullOrWhiteSpace(dto.Category))
                throw new Exception("Категория обязательна");
            if (!dto.IsAdd && account.Balance < dto.TransactionValue)
                throw new Exception("Недостаточно средств на счёте для расхода");
            if (dto.IsAdd && account.Balance + dto.TransactionValue > decimal.MaxValue)
                throw new Exception("Переполнение баланса счёта");
            var t = new Transactions
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                TimeOfTransaction = DateTime.UtcNow,
                AccountId = accountsId,
                IsAdd = dto.IsAdd,
                CreatedAt = DateTime.UtcNow,
                Description = dto.Description
            };
            _context.Transactions.Add(t);
            if (dto.IsAdd)
                account.Balance += dto.TransactionValue;
            else
                account.Balance -= dto.TransactionValue;

            if (!dto.IsAdd)
            {
                var restriction = await _context.Restrictions.FirstOrDefaultAsync(r => r.AccountId == accountsId && r.Category == dto.Category);
                if (restriction != null)
                {
                    restriction.MoneySpent += dto.TransactionValue;
                }
            }
            await _context.SaveChangesAsync();
            return new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction,
                IsAdd = t.IsAdd
            };
        }

        public async Task<bool> UpdateTransactionAsync(int id, TransactionUpdateDto dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            var oldIsAdd = transaction.IsAdd;
            var oldValue = transaction.TransactionValue;
            var oldCategory = transaction.Category;
            var oldAccountId = transaction.AccountId;
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == transaction.AccountId);
            if (!oldIsAdd)
            {
                var oldRestriction = await _context.Restrictions.FirstOrDefaultAsync(r => r.AccountId == oldAccountId && r.Category == oldCategory);
                if (oldRestriction != null)
                {
                    oldRestriction.MoneySpent -= oldValue;
                    if (oldRestriction.MoneySpent < 0) oldRestriction.MoneySpent = 0;
                }
            }
            if (dto.Category != null)
                transaction.Category = dto.Category;
            if (dto.TransactionValue.HasValue)
            {
                if (dto.TransactionValue.Value < 0)
                    throw new Exception("Сумма операции не может быть отрицательной");
                transaction.TransactionValue = dto.TransactionValue.Value;
            }
            if (dto.IsAdd.HasValue)
                transaction.IsAdd = dto.IsAdd.Value;
            if (dto.Description != null)
                transaction.Description = dto.Description;
            if (string.IsNullOrWhiteSpace(transaction.Category))
                return false;
            if (account != null)
            {
                if (!transaction.IsAdd && account.Balance < transaction.TransactionValue)
                    throw new Exception("Недостаточно средств на счёте для расхода");
                if (transaction.IsAdd && account.Balance + transaction.TransactionValue > decimal.MaxValue)
                    throw new Exception("Переполнение баланса счёта");
                if (transaction.IsAdd)
                    account.Balance += transaction.TransactionValue;
                else
                    account.Balance -= transaction.TransactionValue;
            }
            if (!transaction.IsAdd)
            {
                var newRestriction = await _context.Restrictions.FirstOrDefaultAsync(r => r.AccountId == transaction.AccountId && r.Category == transaction.Category);
                if (newRestriction != null)
                {
                    newRestriction.MoneySpent += transaction.TransactionValue;
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == transaction.AccountId);
            if (!transaction.IsAdd)
            {
                var restriction = await _context.Restrictions.FirstOrDefaultAsync(r => r.AccountId == transaction.AccountId && r.Category == transaction.Category);
                if (restriction != null)
                {
                    restriction.MoneySpent -= transaction.TransactionValue;
                    if (restriction.MoneySpent < 0) restriction.MoneySpent = 0;
                }
            }
            if (account != null)
            {
                if (transaction.IsAdd)
                    account.Balance -= transaction.TransactionValue;
                else
                    account.Balance += transaction.TransactionValue;
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TransactionSummaryDto>> GetSummaryByCategoryAsync(int userId, bool? isAdd = null, string? category = null, DateTime? from = null, DateTime? to = null)
        {
            var accounts = await _context.Accounts.Where(a => a.Record != null && a.Record.UserId == userId).ToListAsync();
            var accountIds = accounts.Select(a => a.Id).ToList();
            var query = _context.Transactions.Where(t => accountIds.Contains(t.AccountId));
            if (isAdd.HasValue) query = query.Where(t => t.IsAdd == isAdd);
            if (!string.IsNullOrEmpty(category)) query = query.Where(t => t.Category == category);
            if (from.HasValue) query = query.Where(t => t.TimeOfTransaction >= from);
            if (to.HasValue) query = query.Where(t => t.TimeOfTransaction <= to);
            return await query
                .GroupBy(t => t.Category)
                .Select(g => new TransactionSummaryDto
                {
                    Category = g.Key,
                    TotalTransactions = g.Count(),
                    TotalValue = g.Sum(x => x.TransactionValue)
                })
                .ToListAsync();
        }

        public async Task<List<TransactionDto>> GetTransactionsByRecordAsync(int recordId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == recordId)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Category = t.Category,
                    TransactionValue = t.TransactionValue,
                    TimeOfTransaction = t.TimeOfTransaction,
                    IsAdd = t.IsAdd
                })
                .ToListAsync();
        }

        public async Task<List<TransactionSummaryDto>> GetTopExpensesByCategoryAsync(int userId, int topN, DateTime? from = null, DateTime? to = null)
        {
            var accounts = await _context.Accounts.Where(a => a.Record != null && a.Record.UserId == userId).ToListAsync();
            var accountIds = accounts.Select(a => a.Id).ToList();
            var query = _context.Transactions.Where(t => accountIds.Contains(t.AccountId) && !t.IsAdd);
            if (from.HasValue) query = query.Where(t => t.TimeOfTransaction >= from);
            if (to.HasValue) query = query.Where(t => t.TimeOfTransaction <= to);
            return await query
                .GroupBy(t => t.Category)
                .Select(g => new TransactionSummaryDto
                {
                    Category = g.Key,
                    TotalTransactions = g.Count(),
                    TotalValue = g.Sum(x => x.TransactionValue)
                })
                .OrderByDescending(x => x.TotalValue)
                .Take(topN)
                .ToListAsync();
        }

        public async Task<List<TransactionDto>> FilterTransactionsAsync(int userId, bool? isAdd = null, string? category = null, DateTime? from = null, DateTime? to = null, decimal? min = null, decimal? max = null)
        {
            var accounts = await _context.Accounts.Where(a => a.Record != null && a.Record.UserId == userId).ToListAsync();
            var accountIds = accounts.Select(a => a.Id).ToList();
            var query = _context.Transactions.Where(t => accountIds.Contains(t.AccountId));
            if (isAdd.HasValue) query = query.Where(t => t.IsAdd == isAdd);
            if (!string.IsNullOrEmpty(category)) query = query.Where(t => t.Category == category);
            if (from.HasValue) query = query.Where(t => t.TimeOfTransaction >= from);
            if (to.HasValue) query = query.Where(t => t.TimeOfTransaction <= to);
            if (min.HasValue) query = query.Where(t => t.TransactionValue >= min);
            if (max.HasValue) query = query.Where(t => t.TransactionValue <= max);
            return await query.Select(t => new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction,
                IsAdd = t.IsAdd
            }).ToListAsync();
        }

        public async Task<List<Transactions>> GetTransactionsForExportAsync(int? accountId, int? userId, DateTime? from, DateTime? to)
        {
            var query = _context.Transactions.AsQueryable();
            if (accountId.HasValue)
                query = query.Where(t => t.AccountId == accountId.Value);
            if (userId.HasValue)
                query = query.Where(t => t.Account != null && t.Account.Record != null && t.Account.Record.UserId == userId.Value);
            if (from.HasValue)
                query = query.Where(t => t.TimeOfTransaction >= from.Value);
            if (to.HasValue)
                query = query.Where(t => t.TimeOfTransaction <= to.Value);
            return await query.ToListAsync();
        }

        public async Task<byte[]> ExportTransactionsToExcelAsync(int? accountId, int? userId, DateTime? from, DateTime? to)
        {
            var transactions = await GetTransactionsForExportAsync(accountId, userId, from, to);
            using (var ms = new MemoryStream())
            {
                using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var headerRow = new Row();
                    headerRow.Append(
                        new Cell { CellValue = new CellValue("Id"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("Category"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("TransactionValue"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("TimeOfTransaction"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("IsAdd"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("Description"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("AccountId"), DataType = CellValues.String }
                    );
                    sheetData.AppendChild(headerRow);

                    foreach (var t in transactions)
                    {
                        var row = new Row();
                        row.Append(
                            new Cell { CellValue = new CellValue(t.Id.ToString()), DataType = CellValues.Number },
                            new Cell { CellValue = new CellValue(t.Category ?? ""), DataType = CellValues.String },
                            new Cell { CellValue = new CellValue(t.TransactionValue.ToString()), DataType = CellValues.Number },
                            new Cell { CellValue = new CellValue(t.TimeOfTransaction.ToString("yyyy-MM-dd HH:mm:ss")), DataType = CellValues.String },
                            new Cell { CellValue = new CellValue(t.IsAdd ? "1" : "0"), DataType = CellValues.Number },
                            new Cell { CellValue = new CellValue(t.Description ?? ""), DataType = CellValues.String },
                            new Cell { CellValue = new CellValue(t.AccountId.ToString()), DataType = CellValues.Number }
                        );
                        sheetData.AppendChild(row);
                    }

                    var sheets = new Sheets();
                    sheets.Append(new Sheet
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Transactions"
                    });
                    workbookPart.Workbook.AppendChild(sheets);
                    workbookPart.Workbook.Save();
                }
                return ms.ToArray();
            }
        }

        public async Task<List<TransactionDto>> GetTransactionsByAccountAsync(int accountId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Category = t.Category,
                    TransactionValue = t.TransactionValue,
                    TimeOfTransaction = t.TimeOfTransaction,
                    IsAdd = t.IsAdd
                })
                .ToListAsync();
        }

        public async Task<byte[]> ExportTransactionsToExcelByAccountAsync(int? accountId, int? userId, DateTime? from, DateTime? to)
        {
            var transactions = await GetTransactionsForExportByAccountAsync(accountId, userId, from, to);
            using (var ms = new MemoryStream())
            {
                using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var headerRow = new Row();
                    headerRow.Append(
                        new Cell { CellValue = new CellValue("Id"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("Category"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("TransactionValue"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("TimeOfTransaction"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("IsAdd"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("Description"), DataType = CellValues.String },
                        new Cell { CellValue = new CellValue("AccountId"), DataType = CellValues.String }
                    );
                    sheetData.AppendChild(headerRow);

                    foreach (var t in transactions)
                    {
                        var row = new Row();
                        row.Append(
                            new Cell { CellValue = new CellValue(t.Id.ToString()), DataType = CellValues.Number },
                            new Cell { CellValue = new CellValue(t.Category ?? ""), DataType = CellValues.String },
                            new Cell { CellValue = new CellValue(t.TransactionValue.ToString()), DataType = CellValues.Number },
                            new Cell { CellValue = new CellValue(t.TimeOfTransaction.ToString("yyyy-MM-dd HH:mm:ss")), DataType = CellValues.String },
                            new Cell { CellValue = new CellValue(t.IsAdd ? "1" : "0"), DataType = CellValues.Number },
                            new Cell { CellValue = new CellValue(t.Description ?? ""), DataType = CellValues.String },
                            new Cell { CellValue = new CellValue(t.AccountId.ToString()), DataType = CellValues.Number }
                        );
                        sheetData.AppendChild(row);
                    }

                    var sheets = new Sheets();
                    sheets.Append(new Sheet
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Transactions"
                    });
                    workbookPart.Workbook.AppendChild(sheets);
                    workbookPart.Workbook.Save();
                }
                return ms.ToArray();
            }
        }

        public async Task<List<Transactions>> GetTransactionsForExportByAccountAsync(int? accountId, int? userId, DateTime? from, DateTime? to)
        {
            var query = _context.Transactions.AsQueryable();
            if (accountId.HasValue)
                query = query.Where(t => t.AccountId == accountId.Value);
            if (userId.HasValue)
                query = query.Where(t => t.Account != null && t.Account.Record != null && t.Account.Record.UserId == userId.Value);
            if (from.HasValue)
                query = query.Where(t => t.TimeOfTransaction >= from.Value);
            if (to.HasValue)
                query = query.Where(t => t.TimeOfTransaction <= to.Value);
            return await query.ToListAsync();
        }
    }
}
