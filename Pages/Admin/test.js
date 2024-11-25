const menuItems = [
    { name: "Cà Phê Sữa Đá", category: "coffee", price: 35000, image: "coffee.jpg" },
    { name: "Trà Đào Cam Sả", category: "tea", price: 45000, image: "tea.jpg" },
    { name: "Bánh Mì Gà", category: "snack", price: 30000, image: "banhmi.jpg" },
    { name: "Espresso", category: "coffee", price: 40000, image: "espresso.jpg" },
    { name: "Trà Sữa Trân Châu", category: "tea", price: 50000, image: "milktea.jpg" },
    { name: "Bánh Su Kem", category: "snack", price: 20000, image: "pastry.jpg" },
    { name: "Cappuccino", category: "coffee", price: 45000, image: "cappuccino.jpg" },
    { name: "Trà Chanh", category: "tea", price: 25000, image: "lemontea.jpg" },
    { name: "Mochaccino", category: "coffee", price: 50000, image: "mochaccino.jpg" },
    { name: "Matcha Latte", category: "tea", price: 55000, image: "matcha.jpg" },
    { name: "Bánh Croissant", category: "snack", price: 35000, image: "croissant.jpg" },
    { name: "Americano", category: "coffee", price: 30000, image: "americano.jpg" },
];

let currentPage = 1;
const itemsPerPage = 12;

function renderMenuItems() {
    const grid = document.getElementById("menuGrid");
    const searchInput = document.getElementById("searchInput").value.toLowerCase();
    const categoryFilter = document.getElementById("categoryFilter").value;

    const filteredItems = menuItems.filter(item => 
        (item.name.toLowerCase().includes(searchInput) || searchInput === "") &&
        (item.category === categoryFilter || categoryFilter === "all")
    );

    const start = (currentPage - 1) * itemsPerPage;
    const end = currentPage * itemsPerPage;

    grid.innerHTML = "";
    filteredItems.slice(start, end).forEach(item => {
        grid.innerHTML += `
            <div class="menu-item">
                <img src="${item.image}" alt="${item.name}" class="menu-image">
                <h3 class="menu-name">${item.name}</h3>
                <p class="menu-price">${item.price.toLocaleString()}₫</p>
            </div>
        `;
    });

    document.getElementById("pageIndicator").textContent = `${currentPage} / ${Math.ceil(filteredItems.length / itemsPerPage)}`;
}

document.getElementById("searchInput").addEventListener("input", renderMenuItems);
document.getElementById("categoryFilter").addEventListener("change", renderMenuItems);
document.getElementById("prevPage").addEventListener("click", () => {
    if (currentPage > 1) currentPage--;
    renderMenuItems();
});
document.getElementById("nextPage").addEventListener("click", () => {
    const totalItems = menuItems.length;
    if (currentPage < Math.ceil(totalItems / itemsPerPage)) currentPage++;
    renderMenuItems();
});

renderMenuItems();

document.addEventListener("DOMContentLoaded", function () {
    const menuItems = document.querySelectorAll(".menu-item"); // Lấy tất cả các món
    const itemsPerPage = 12; // Hiển thị 12 món mỗi trang
    let currentPage = 1;

    function renderMenu(page) {
        menuItems.forEach((item, index) => {
            item.style.display =
                index >= (page - 1) * itemsPerPage && index < page * itemsPerPage
                    ? "block"
                    : "none";
        });
    }

    renderMenu(currentPage);

    document.getElementById("prevPage").addEventListener("click", () => {
        if (currentPage > 1) {
            currentPage--;
            renderMenu(currentPage);
        }
    });

    document.getElementById("nextPage").addEventListener("click", () => {
        if (currentPage * itemsPerPage < menuItems.length) {
            currentPage++;
            renderMenu(currentPage);
        }
    });
});
