(function () {
    function setBearerToken(token) {
        function trySet() {
            if (window.ui && window.ui.preauthorizeApiKey) {
                const fullToken = 'Bearer ' + token;
                console.log('[Swagger-AutoToken] Подставляется токен:', fullToken);
                window.ui.preauthorizeApiKey('Bearer', fullToken);
                console.log('Токен автоматически подставлен в Swagger UI');
            } else {
                setTimeout(trySet, 300);
            }
        }
        trySet();
    }

    const oldFetch = window.fetch;
    window.fetch = async function() {
        const response = await oldFetch.apply(this, arguments);
        try {
            const url = arguments[0];
            if (typeof url === 'string' && (url.endsWith('/api/Auth/login') || url.endsWith('/api/Auth/register'))) {
                const cloned = response.clone();
                const data = await cloned.json();
                const token = data.token || data.Token || data.accessToken;
                if (token) {
                    setBearerToken(token);
                }
            }
        } catch (e) {}
        return response;
    };
})();
