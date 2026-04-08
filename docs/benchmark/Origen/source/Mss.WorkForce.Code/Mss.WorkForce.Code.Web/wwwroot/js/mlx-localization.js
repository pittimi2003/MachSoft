class JavaScriptLocalization {
    constructor() {
        this.resources = {};
        this.baseUrl = '/Localization';
        this.enableLocalization = false
    }

    initialize(resourcesDictionary, enableLocalization) {
        try {
            if (!resourcesDictionary) {
                console.error('No resources dictionary provided for initialization');
                return false;
            }

            this.resources = { ...resourcesDictionary };
            this.enableLocalization = enableLocalization;

            return true;
        } catch (error) {
            console.error('Error initializing JavaScript localization:', error);
            return false;
        }
    }

    // Obtener string localizado
    getString(key, ...args) {
        
        const normalizedKey = this.generateNormalizedKey(key);

        let resultValue = this.resources[normalizedKey];

        if (!resultValue && this.enableLocalization) {
            // Si no existe, añadirlo al archivo de recursos
            this.addMissingKeyAsync(normalizedKey, key);
            resultValue = `* ${key}`;
        }
        else if (!resultValue)
        {
            resultValue = key;
        }

        // verificar si tiene argumentos, retornar el recuso sin formatear 
        if (args.length === 0) {
            return resultValue;
        }

        resultValue = this.formatResourceWithArguments(resultValue, ...args);

        return resultValue;
    }

    // Agregar clave faltante
    async addMissingKeyAsync(key, value) {
        try {
            const response = await fetch(`${this.baseUrl}/AddMissingKey`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ key: key, value: value })
            });

            if (response.ok) {
                this.resources[key] = value;
                return true;
            } else {
                console.error(`Failed to add JavaScript key: ${response.message}`);
                return false;
            }
        } catch (error) {
            console.error('Error adding JavaScript key:', error);
            return false;
        }
    }

    // normalizar las key de los recursos
    generateNormalizedKey(key) {
        if (!key) {
            return key;
        }

        const originalKey = key.toString();

        let cleanKey = originalKey
            .trim()
            .toUpperCase()
            .replace(/\s+/g, '')
            .replace(/[^A-Z0-9]/g, '_') // Quitar caracteres especionales

        // Agrego prefijo JS. si no lo tiene
        const finalKey = cleanKey.startsWith('JS.') ? cleanKey : `JS.${cleanKey}`;

        return finalKey;
    }

    // Formatear el recurso con argumentos
    formatResourceWithArguments(resourceText, ...args) {

        try {
            if (!resourceText || args.length === 0) {
                return resourceText;
            }

            let resultValue = resourceText;

            args.forEach((value, index) => {
                const pattern = `{${index}}`;
                resultValue = resultValue.replace(pattern, value.toString());
            });

            return resultValue;

        } catch(ex) {
            console.error('Error formatting resource', value, ex);
            return resourceText;
        }
    }
}

// Crear instancia global
window.jsLocalization = new JavaScriptLocalization();

// Función global para inicializar desde Blazor
window.initializeJavaScriptLocalization = function (resourcesDictionary, enableLocalization) {
    return window.jsLocalization.initialize(resourcesDictionary, enableLocalization);
};

// Función global para buscar el recurso
window.L = function (key, ...args) {
    return window.jsLocalization.getString(key, ...args);
};


