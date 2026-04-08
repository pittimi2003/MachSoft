// Función para cargar el contenido del archivo CSS
window.fetchCss = async (url) => {
    const response = await fetch(url);
    if (!response.ok) {
        console.error(`No se pudo cargar el archivo CSS desde ${url}`);
        return "";
    }
    return await response.text();
};

// Función para inyectar estilos en el documento
window.injectStyles = (cssContent, uniqueId) => {
    // Verificar si los estilos ya están inyectados
    if (document.getElementById(uniqueId)) {
        return;
    }

    // Crear un nuevo elemento <style>
    const styleElement = document.createElement("style");
    styleElement.id = uniqueId; // ID único para evitar duplicados
    styleElement.type = "text/css";
    styleElement.innerHTML = cssContent;

    // Agregar el elemento <style> al head del documento
    document.head.appendChild(styleElement);
};
