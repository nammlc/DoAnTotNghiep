// Fake dữ liệu thống kê
const data = {
    tongDoanhThu: 50000000,
    tongLoiNhuan: 20000000,
    tongLuongNhanVien: 10000000,
    doanhThuThoiGian: {
        labels: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5"],
        values: [8000000, 9000000, 7500000, 8500000, 7000000]
    },
    doanhThuMon: {
        labels: ["Cà phê", "Trà sữa", "Sinh tố", "Nước ép", "Trà đào"],
        values: [15000000, 20000000, 12000000, 10000000, 8000000]
    }
};

// Hiển thị dữ liệu tổng quan
document.getElementById("tong-doanh-thu").textContent = `${data.tongDoanhThu.toLocaleString()} VNĐ`;
document.getElementById("tong-loi-nhuan").textContent = `${data.tongLoiNhuan.toLocaleString()} VNĐ`;
document.getElementById("tong-luong-nhan-vien").textContent = `${data.tongLuongNhanVien.toLocaleString()} VNĐ`;

// Biểu đồ Doanh Thu Theo Thời Gian
const ctxThoiGian = document.getElementById('doanhThuThoiGian').getContext('2d');
new Chart(ctxThoiGian, {
    type: 'line',
    data: {
        labels: data.doanhThuThoiGian.labels,
        datasets: [{
            label: 'Doanh Thu (VNĐ)',
            data: data.doanhThuThoiGian.values,
            borderColor: 'rgba(75, 192, 192, 1)',
            backgroundColor: 'rgba(75, 192, 192, 0.2)',
            borderWidth: 2
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: { position: 'top' },
            title: { display: true, text: 'Doanh Thu Theo Thời Gian' }
        }
    }
});

// Biểu đồ Doanh Thu Theo Món
const ctxMon = document.getElementById('doanhThuMon').getContext('2d');
new Chart(ctxMon, {
    type: 'bar',
    data: {
        labels: data.doanhThuMon.labels,
        datasets: [{
            label: 'Doanh Thu Theo Món (VNĐ)',
            data: data.doanhThuMon.values,
            backgroundColor: 'rgba(153, 102, 255, 0.6)',
            borderColor: 'rgba(153, 102, 255, 1)',
            borderWidth: 2
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: { position: 'top' },
            title: { display: true, text: 'Doanh Thu Theo Món' }
        }
    }
});
