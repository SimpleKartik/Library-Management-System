document.addEventListener("DOMContentLoaded", function () {
    
    // --- Sidebar Toggle ---
    const sidebar = document.getElementById("sidebar");
    const sidebarCollapse = document.getElementById("sidebarCollapse");

    if (sidebarCollapse) {
        sidebarCollapse.addEventListener("click", function () {
            sidebar.classList.toggle("active");
        });
    }

    // --- Dark Mode Toggle ---
    const themeToggleBtn = document.getElementById("themeToggle");
    const themeIcon = themeToggleBtn ? themeToggleBtn.querySelector("i") : null;
    
    // Check localStorage for saved theme
    const currentTheme = localStorage.getItem("theme") || "light";
    document.documentElement.setAttribute("data-theme", currentTheme);
    updateThemeIcon(currentTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener("click", function () {
            let theme = document.documentElement.getAttribute("data-theme");
            
            if (theme === "dark") {
                theme = "light";
            } else {
                theme = "dark";
            }
            
            document.documentElement.setAttribute("data-theme", theme);
            localStorage.setItem("theme", theme);
            updateThemeIcon(theme);
        });
    }

    function updateThemeIcon(theme) {
        if (!themeIcon) return;
        if (theme === "dark") {
            themeIcon.classList.remove("fa-moon");
            themeIcon.classList.add("fa-sun");
            themeIcon.classList.add("text-warning");
        } else {
            themeIcon.classList.remove("fa-sun");
            themeIcon.classList.remove("text-warning");
            themeIcon.classList.add("fa-moon");
        }
    }

    // --- SweetAlert2 for Delete Forms ---
    const deleteForms = document.querySelectorAll(".form-delete");
    deleteForms.forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            
            const message = form.getAttribute("data-confirm-message") || "Are you sure you want to delete this item?";
            
            Swal.fire({
                title: 'Are you sure?',
                text: message,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    });
});
