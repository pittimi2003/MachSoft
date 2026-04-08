//workingMode: "Designer" | "Viewer" | "ViewerOnly"
export function initializeDashboard() {
    var dashboardControl = new DevExpress.Dashboard.DashboardControl(document.getElementById("web-dashboard"), {
        endpoint: "/api/dashboardMetrics",
        workingMode: "ViewerOnly" 

    });
    dashboardControl.render();
}