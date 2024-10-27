
document.addEventListener("DOMContentLoaded", function () {
    var toggleSidebar = document.getElementById("toggleSidebar");
    var sidebar = document.querySelector(".sidebar");
    var mainContent = document.querySelector(".main-content");

    toggleSidebar.addEventListener("click", function () {
        if (sidebar.style.width === "65px") {
            sidebar.style.width = "200px"; 
            mainContent.style.paddingLeft = "20px";
            toggleSidebar.innerHTML = '<i class="fa-solid fa-chevron-left" style="padding-right : 10px "></i> Thu gọn'; 
        } else {
            sidebar.style.width = "65px"; 
            mainContent.style.paddingLeft = "30px";
            toggleSidebar.innerHTML = '<i class="fa-solid fa-chevron-right"></i>'; 
        }
    });
});