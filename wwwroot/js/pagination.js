document.addEventListener("DOMContentLoaded", function() {
    const staffTableBody = document.getElementById('staff-table-body');
    const pagination = document.getElementById('pagination');
    const searchInput = document.getElementById('searchInput');

    const data = [];
    for (let i = 1; i <= 100; i++) {
        data.push({
            stt: i,
            maNhanVien: `NV${i.toString().padStart(3, '0')}`,
            hoTen: `Nguyễn Văn ${String.fromCharCode(64 + (i % 26))}`,
            gioiTinh: i % 2 === 0 ? 'Nam' : 'Nữ',
            diaChi: i % 2 === 0 ? 'Hà Nội' : 'TP. Hồ Chí Minh',
            ngaySinh: `01/01/${1985 + (i % 35)}`,
            email: `nv${i}@gmail.com`
        });
    }

    let filteredData = [...data]; 

    const itemsPerPage = 10;
    let currentPage = 1;

    function displayPage(page) {
        currentPage = page;
        const start = (page - 1) * itemsPerPage;
        const end = start + itemsPerPage;
        staffTableBody.innerHTML = '';

        for (let i = start; i < end; i++) {
            if (filteredData[i]) {
                const row = `<tr data-index="${i}">
                    <td>${filteredData[i].stt}</td>
                    <td>${filteredData[i].maNhanVien}</td>
                    <td>${filteredData[i].hoTen}</td>
                    <td>${filteredData[i].gioiTinh}</td>
                    <td>${filteredData[i].diaChi}</td>
                    <td>${filteredData[i].ngaySinh}</td>
                    <td style="display :flex ; justify-content: space-between; align-items: center;"><div> ${filteredData[i].email} </div> <div class="hide-icon_form"> <i class="fa-solid fa-pen" style="color: #50B83C"></i> <i class="fa-solid fa-user-plus" style="color: #50B83C"></i> <i class="fa-solid fa-xmark" style="color: red"></i> </div></td>
                </tr>`;
                staffTableBody.innerHTML += row;
            }
        }

        
        staffTableBody.querySelectorAll('tr').forEach(row => {
            row.addEventListener('click', function() {
                const index = parseInt(row.getAttribute('data-index'));
                const staff = filteredData[index];
                showDetailPage(staff); 
            });
        });
    }

    function setupPagination() {
        pagination.innerHTML = '';
        const firstButton = `<li class="page-item">
            <a class="page-link" href="#" data-page="1"> <img src="~/css/assets/icon/btn-firstpage.svg"> </a>
        </li>`;
        pagination.innerHTML += firstButton;

        const prevButton = `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${currentPage - 1}"> <img src="~/css/assets/icon/btn-prev-page.svg"> </a>
        </li>`;
        pagination.innerHTML += prevButton;

        const dropdownMenu = `<li class="page-item dropdown" style=" padding : 0">
            <a class="page-link dropdown-toggle" href="#" role="button" id="dropdownMenuLink" data-bs-toggle="dropdown" aria-expanded="false">
                 ${currentPage}
            </a>
            <ul class="dropdown-menu" aria-labelledby="dropdownMenuLink">
                ${generatePageDropdown()}
            </ul>
        </li>`;
        pagination.innerHTML += dropdownMenu;

        const nextButton = `<li class="page-item ${currentPage === Math.ceil(filteredData.length / itemsPerPage) ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${currentPage + 1}"> <img src="~/css/assets/icon/btn-next-page.svg"> </a>
        </li>`;
        pagination.innerHTML += nextButton;

        const lastButton = `<li class="page-item">
            <a class="page-link" href="#" data-page="${Math.ceil(filteredData.length / itemsPerPage)}"> <img src="~/css/assets/icon/btn-lastpage.svg"> </a>
        </li>`;
        pagination.innerHTML += lastButton;
        
        pagination.querySelectorAll('.page-link').forEach(link => {
            link.addEventListener('click', function(event) {
                event.preventDefault();
                let page = parseInt(event.currentTarget.getAttribute('data-page'));
                if (isNaN(page)) {
                    page = parseInt(event.currentTarget.querySelector('img').parentElement.getAttribute('data-page'));
                }
                if (page >= 1 && page <= Math.ceil(filteredData.length / itemsPerPage)) {
                    displayPage(page);
                    setupPagination();
                }
            });
        });

        pagination.querySelectorAll('.dropdown-item').forEach(item => {
            item.addEventListener('click', function(event) {
                event.preventDefault();
                const page = parseInt(event.target.getAttribute('data-page'));
                displayPage(page);
                setupPagination();
            });
        });
    }

    function generatePageDropdown() {
        let dropdownHTML = '';
        for (let i = 1; i <= Math.ceil(filteredData.length / itemsPerPage); i++) {
            dropdownHTML += `<li><a class="dropdown-item" href="#" data-page="${i}">Trang ${i}</a></li>`;
        }
        return dropdownHTML;
    }

    function showDetailPage(staff) {
        
        window.location.href = `DetailStaff.html?id=${staff.maNhanVien}`;
    }

    searchInput.addEventListener('input', function(event) {
        const searchText = event.target.value.trim().toLowerCase();
        filteredData = data.filter(item =>
            item.maNhanVien.toLowerCase().includes(searchText) ||
            item.hoTen.toLowerCase().includes(searchText) ||
            item.email.toLowerCase().includes(searchText)
        );

        displayPage(1);
        setupPagination();
    });

    displayPage(1);
    setupPagination();
});
