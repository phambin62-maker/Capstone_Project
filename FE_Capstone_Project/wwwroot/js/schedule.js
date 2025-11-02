// wwwroot/js/schedule.js
document.addEventListener('DOMContentLoaded', function () {
    // Date validation
    const departureDate = document.getElementById('DepartureDate');
    const arrivalDate = document.getElementById('ArrivalDate');

    if (departureDate && arrivalDate) {
        departureDate.addEventListener('change', validateDates);
        arrivalDate.addEventListener('change', validateDates);
    }

    function validateDates() {
        const departure = new Date(departureDate.value);
        const arrival = new Date(arrivalDate.value);

        if (departure >= arrival) {
            arrivalDate.setCustomValidity('Ngày kết thúc phải sau ngày khởi hành');
        } else {
            arrivalDate.setCustomValidity('');
        }
    }
});