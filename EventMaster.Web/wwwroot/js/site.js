(function () {
    const typeToClass = {
        success: "text-bg-success",
        danger: "text-bg-danger",
        warning: "text-bg-warning",
        info: "text-bg-info"
    };

    function resolveType(type) {
        return typeToClass[type] ? type : "info";
    }

    function ensureContainer() {
        return document.getElementById("em-toast-container");
    }

    window.emNotify = function (message, type = "info", delay = 5000) {
        if (!message || !window.bootstrap) {
            return;
        }

        const container = ensureContainer();
        if (!container) {
            return;
        }

        const resolvedType = resolveType(type);
        const toast = document.createElement("div");
        toast.className = `toast align-items-center border-0 ${typeToClass[resolvedType]}`;
        toast.setAttribute("role", "status");
        toast.setAttribute("aria-live", "polite");
        toast.setAttribute("aria-atomic", "true");
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>`;

        container.appendChild(toast);
        const bsToast = new bootstrap.Toast(toast, { delay: Number(delay) || 5000 });
        toast.addEventListener("hidden.bs.toast", () => toast.remove());
        bsToast.show();
    };

    document.addEventListener("DOMContentLoaded", function () {
        const queued = window.emInitialNotifications || [];
        queued.forEach((item) => window.emNotify(item.message, item.type, item.delay));
    });
})();
