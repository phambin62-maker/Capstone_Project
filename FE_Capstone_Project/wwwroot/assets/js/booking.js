'use strict';

(function () {
    document.addEventListener("DOMContentLoaded", function () {
        const step3Button = document.querySelector('[data-bs-target="#travel-booking-step-3"]');
        const travelersList = document.getElementById("travelers-list");

        let previousAdults = 0;
        let previousChildren = 0;
        let previousInfants = 0;

        const adultsInput = document.getElementById("adults");
        const childrenInput = document.getElementById("children");
        const infantsInput = document.getElementById("infants");

        const form = document.querySelector('.booking-form');
        const validationMessageEl = document.getElementById('form-validation-message');

        const visitedSteps = new Set([0]);

        forceNumericInput(adultsInput);
        forceNumericInput(childrenInput);
        forceNumericInput(infantsInput);

        // Disable inputs at the beginning
        adultsInput.disabled = true;
        childrenInput.disabled = true;
        infantsInput.disabled = true;

        step3Button.addEventListener("click", function () {
            generateTravelers();
        });

        function markRequiredFields() {
            document.querySelectorAll('input[required], select[required], textarea[required]').forEach(el => {
                if (el.disabled) return;
                const id = el.id;
                let label = id ? document.querySelector('label[for="' + id + '"]') : null;
                if (!label) {
                    const parentLabel = el.closest('div')?.querySelector('label');
                    label = parentLabel || null;
                }
                if (label && !label.querySelector('.required-star')) {
                    const star = document.createElement('span');
                    star.className = 'required-star';
                    star.textContent = '*';
                    label.appendChild(star);
                }
            });
        }

        function createTravelerForm(index, indexNumber, type) {
            return `
                    <div class="col-md-12">
                        <div class="card p-4 shadow-sm border-0 rounded-3 mb-4">
                            <h4>${type} ${indexNumber}</h4>

                            <div class="row g-4">
                                <div class="col-md-6">
                                    <label class="form-label">First Name</label>
                                    <input type="text" name="Travelers[${index}].FirstName" class="form-control" required>
                                </div>

                                <div class="col-md-6">
                                    <label class="form-label">Last Name</label>
                                    <input type="text" name="Travelers[${index}].LastName" class="form-control" required>
                                </div>

                                <div class="col-md-6">
                                    <label class="form-label">Email</label>
                                    <input type="email" name="Travelers[${index}].Email" class="form-control" required>
                                </div>

                                <div class="col-md-6">
                                    <label class="form-label">Phone Number</label>
                                    <input type="text" name="Travelers[${index}].PhoneNumber" class="form-control" required>
                                </div>

                                <div class="col-md-12">
                                    <label class="form-label">Identity Number</label>
                                    <input type="text" name="Travelers[${index}].IdentityID" class="form-control">
                                </div>
                            </div>

                            <input type="hidden" name="Travelers[${index}].CustomerType" value="${type}">
                        </div>
                    </div>
                `;
        }

        function generateTravelers() {
            const adults = parseInt(document.getElementById("adults").value) || 0;
            const children = parseInt(document.getElementById("children").value) || 0;
            const infants = parseInt(document.getElementById("infants").value) || 0;

            if (adults === previousAdults && children === previousChildren && infants === previousInfants) {
                return;
            }

            previousAdults = adults;
            previousChildren = children;
            previousInfants = infants;

            travelersList.innerHTML = "";

            // Adults
            for (let i = 0; i < adults; i++) {
                travelersList.innerHTML += createTravelerForm(i, i + 1, "Adult");
            }

            // Children
            for (let i = 0; i < children; i++) {
                travelersList.innerHTML += createTravelerForm(adults + i, i + 1, "Child");
            }

            // Infants
            for (let i = 0; i < infants; i++) {
                travelersList.innerHTML += createTravelerForm(adults + children + i, i + 1, "Infant");
            }

            if (adults + children + infants === 0) {
                travelersList.innerHTML = `<p class="text-muted">Please enter the number of travelers in Step 1.</p>`;
            }

            markRequiredFields();
            attachFieldListeners();
        }

        function updateSummary() {
            const numAdultsEl = document.getElementById("adults");
            const numChildrenEl = document.getElementById("children");
            const numInfantsEl = document.getElementById("infants");
            const pricePerAdultEl = document.getElementById("pricePerAdult");
            const pricePerChildEl = document.getElementById("pricePerChild");
            const groupDiscountEl = document.getElementById("groupDiscount");
            const totalPriceEl = document.getElementById("totalPrice");
            const totalTravelersEl = document.getElementById("totalTravelers");

            // read current values (may be clamped below)
            let adults = parseInt(numAdultsEl.value) || 0;
            let children = parseInt(numChildrenEl.value) || 0;
            let infants = parseInt(numInfantsEl.value) || 0;

            const scheduleId = document.getElementById("tour-schedule").value;
            const available = calculateAvailableSeats(scheduleId);

            const seatEl = document.getElementById("availableSeats");

            // if schedule not selected, show defaults
            if (scheduleId === "-1") {
                document.getElementById("availableSeats").textContent = "-";
                document.getElementById("numAdults").textContent = 0;
                document.getElementById("numChildren").textContent = 0;
                document.getElementById("totalTravelers").textContent = 0;
                document.getElementById("pricePerAdult").textContent = "0 VND";
                document.getElementById("pricePerChild").textContent = "0 VND";
                document.getElementById("groupDiscount").textContent = "0 VND";
                document.getElementById("totalPrice").textContent = "0 VND";
                const btnBookDefault = document.querySelector(".btn-book");
                if (btnBookDefault) btnBookDefault.disabled = true;
                return;
            }

            // update remaining seats text (before adjustments)
            const totalBeforeClamp = adults + children + infants;
            const remainingBefore = available !== null ? available - totalBeforeClamp : 0;
            seatEl.textContent = remainingBefore >= 0 ? remainingBefore : "0";

            // clamp inputs so sum doesn't exceed available seats
            if (available !== null) {
                const maxAllowed = available;

                setMaxAndClamp(numAdultsEl, Math.max(0, maxAllowed));
                adults = parseInt(numAdultsEl.value) || 0;

                setMaxAndClamp(numChildrenEl, Math.max(0, maxAllowed - adults));
                children = parseInt(numChildrenEl.value) || 0;

                setMaxAndClamp(numInfantsEl, Math.max(0, maxAllowed - adults - children));
                infants = parseInt(numInfantsEl.value) || 0;

                let sum = adults + children + infants;
                if (sum > maxAllowed) {
                    const overflow = sum - maxAllowed;
                    if (children >= overflow) {
                        children -= overflow;
                    } else {
                        const remain = overflow - children;
                        children = 0;
                        infants = Math.max(0, infants - remain);
                    }
                    numChildrenEl.value = children;
                    numInfantsEl.value = infants;
                }

                // update displayed available seats after clamp
                const remainingAfter = maxAllowed - (adults + children + infants);
                seatEl.textContent = remainingAfter >= 0 ? remainingAfter : "0";
            }

            const totalTravelers = adults + children + infants;

            const btnBook = document.querySelector(".btn-book");
            if (btnBook) btnBook.disabled = (scheduleId === "-1" || totalTravelers === 0 || (available !== null && (totalTravelers > available)));

            // Pricing calculations
            const childPrice = (tour.price || 0) * (1 - (tour.childDiscount || 0) / 100);
            let totalPrice = (adults * (tour.price || 0)) + ((children + infants) * childPrice);
            let groupDiscountAmount = 0;
            const groupThreshold = (tour.groupNumber && tour.groupNumber > 0) ? tour.groupNumber : Infinity;

            if (totalTravelers >= groupThreshold && (tour.groupDiscount || 0) > 0) {
                groupDiscountAmount = totalPrice * ((tour.groupDiscount || 0) / 100);
                totalPrice -= groupDiscountAmount;
            }

            const totalAdultPrice = adults * (tour.price || 0);
            const totalChildPrice = children * childPrice;

            // Update display
            pricePerAdultEl.textContent = totalAdultPrice.toLocaleString('en-US') + " VND";
            pricePerChildEl.textContent = totalChildPrice.toLocaleString('en-US') + " VND";
            if (groupDiscountAmount !== 0) {
                groupDiscountEl.textContent = "-" + groupDiscountAmount.toLocaleString('en-US') + " VND";
            } else {
                groupDiscountEl.textContent = "0 VND";
            }
            totalPriceEl.textContent = totalPrice.toLocaleString('en-US') + " VND";
            totalTravelersEl.textContent = totalTravelers;
            // show combined children+infants where UI expects it
            document.getElementById("numAdults").textContent = adults;
            document.getElementById("numChildren").textContent = (children + infants);
        }

        //set tour schedule text
        document.getElementById("tour-schedule").addEventListener("change", function () {
            const selectedScheduleId = this.value;

            const spanToChange = document.getElementById("tSchedule");

            if (selectedScheduleId && selectedScheduleId !== "-1") {

                // ENABLE inputs when schedule selected
                adultsInput.disabled = false;
                childrenInput.disabled = false;
                infantsInput.disabled = false;

                spanToChange.textContent = this.options[this.selectedIndex].text.trim();

                const available = calculateAvailableSeats(selectedScheduleId);
                document.getElementById("availableSeats").textContent =
                    available >= 0 ? available : "N/A";

            } else {
                spanToChange.textContent = "N/A";

                // DISABLE & RESET inputs when schedule deselected
                adultsInput.disabled = true;
                childrenInput.disabled = true;
                infantsInput.disabled = true;

                adultsInput.value = 0;
                childrenInput.value = 0;
                infantsInput.value = 0;

                travelersList.innerHTML = "";
            }

            updateSummary();
        });


        document.getElementById("adults").addEventListener("input", function () {
            const scheduleId = document.getElementById("tour-schedule").value;
            const maxSeats = calculateAvailableSeats(scheduleId);
            let val = parseInt(this.value) || 0;

            if (val > maxSeats) {
                val = maxSeats;
                this.value = val;
            }

            document.getElementById("numAdults").textContent = val;
            updateSummary();
        });

        document.getElementById("children").addEventListener("input", function () {
            const scheduleId = document.getElementById("tour-schedule").value;
            const maxSeats = calculateAvailableSeats(scheduleId) || 0;

            let children = parseInt(this.value) || 0;
            const adults = parseInt(document.getElementById("adults").value) || 0;
            const infants = parseInt(document.getElementById("infants").value) || 0;

            if (adults + children + infants > maxSeats) {
                children = maxSeats - adults - infants;
                if (children < 0) children = 0;
                this.value = children;
            }

            document.getElementById("numChildren").textContent = children + infants;
            updateSummary();
        });

        document.getElementById("infants").addEventListener("input", function () {
            const scheduleId = document.getElementById("tour-schedule").value;
            const maxSeats = calculateAvailableSeats(scheduleId) || 0;

            let infants = parseInt(this.value) || 0;
            const adults = parseInt(document.getElementById("adults").value) || 0;
            const children = parseInt(document.getElementById("children").value) || 0;

            if (adults + children + infants > maxSeats) {
                infants = maxSeats - adults - children;
                if (infants < 0) infants = 0;
                this.value = infants;
            }

            document.getElementById("numChildren").textContent = children + infants;
            updateSummary();
        });

        function calculateAvailableSeats(scheduleId) {
            if (scheduleId === "-1") return null;
            const scheduleIdNum = parseInt(scheduleId);
            const match = bookedSeats.find(x => x.TourScheduleId === scheduleIdNum);
            const booked = match ? match.BookedSeats : 0;
            return tour.maxSeats - booked;
        }

        function setMaxAndClamp(input, max) {
            input.max = max;

            if (parseInt(input.value) > max) {
                input.value = max;
            }
        }

        // Step navigation
        const steps = document.querySelectorAll(".form-step");
        const stepTabs = document.querySelectorAll(".step.nav-link");
        const nextButtons = document.querySelectorAll(".next-button");
        const prevButtons = document.querySelectorAll(".prev-button");
        let currentStep = 0;

        function showStep(index) {
            steps.forEach(step => step.classList.remove("show", "active"));
            steps[index].classList.add("show", "active");

            stepTabs.forEach((tab, i) => {
                if (i === index) {
                    tab.classList.add("active");
                    tab.setAttribute("aria-selected", "true");
                    tab.removeAttribute("tabindex");
                } else {
                    tab.classList.remove("active");
                    tab.setAttribute("aria-selected", "false");
                    tab.setAttribute("tabindex", "-1");
                }
            });

            visitedSteps.add(index);
        }

        function showValidationMessage(text) {
            validationMessageEl.textContent = text;
            validationMessageEl.classList.remove('d-none');
        }

        function hideValidationMessage() {
            validationMessageEl.textContent = '';
            validationMessageEl.classList.add('d-none');
        }

        function getMissingRequiredInStep(index) {
            const missing = [];
            const stepEl = steps[index];
            if (!stepEl) return missing;

            const requiredEls = stepEl.querySelectorAll('input[required], select[required], textarea[required]');
            requiredEls.forEach(el => {
                if (el.disabled) return;
                const tag = el.tagName.toLowerCase();
                const type = (el.type || '').toLowerCase();
                const val = el.value == null ? '' : String(el.value).trim();

                let invalid = false;
                if (tag === 'select' && val === '-1') invalid = true;
                else if (val === '') {
                    invalid = true;
                }
                else if (type === 'email' && !isValidEmail(val)) {
                    invalid = true;
                }

                if (invalid) {
                    el.classList.add('is-invalid');
                    const label = el.id ? document.querySelector('label[for="' + el.id + '"]') : el.closest('div')?.querySelector('label');
                    const name = label ? label.textContent.trim().replace('*', '').trim() : (el.name || 'Field');
                    if (type === 'email' && val !== '' && !isValidEmail(val)) {
                        missing.push(name + ' (invalid email)');
                    } else {
                        missing.push(name);
                    }
                } else {
                    el.classList.remove('is-invalid');
                }
            });

            if (index === 0) {
                const schedule = document.getElementById("tour-schedule").value;
                const adults = parseInt(document.getElementById("adults").value) || 0;
                const children = parseInt(document.getElementById("children").value) || 0;
                const infants = parseInt(document.getElementById("infants").value) || 0;

                if (schedule === '-1' && !missing.includes('Tour schedule')) {
                    const sel = document.getElementById('tour-schedule');
                    sel.classList.add('is-invalid');
                    missing.unshift('Tour schedule');
                }
                if (adults + children + infants === 0 && !missing.includes('At least one traveler')) {
                    document.getElementById('adults').classList.add('is-invalid');
                    document.getElementById('children').classList.add('is-invalid');
                    document.getElementById('infants').classList.add('is-invalid');
                    missing.push('At least one traveler (Adults/Children/Infants)');
                }
            }

            return missing;
        }

        function validateStep(index) {
            const missing = getMissingRequiredInStep(index);
            if (missing.length > 0) {
                const unique = [...new Set(missing)];
                showValidationMessage('Please fill required fields: ' + unique.join(', '));
                return false;
            }
            return true;
        }

        function getMissingRequiredAcrossVisitedSteps() {
            const missing = [];
            visitedSteps.forEach(idx => {
                getMissingRequiredInStep(idx).forEach(m => missing.push(m));
            });
            return [...new Set(missing)];
        }

        function attachFieldListeners() {
            form.querySelectorAll('input, select, textarea').forEach(el => {
                if (el.__listenerAttached) return;
                el.addEventListener('input', onFieldChange);
                el.addEventListener('change', onFieldChange);
                el.__listenerAttached = true;
            });
        }

        function onFieldChange() {
            this.classList.remove('is-invalid');
            if (!validationMessageEl.classList.contains('d-none')) {
                const miss = getMissingRequiredAcrossVisitedSteps();
                if (miss.length === 0) {
                    hideValidationMessage();
                } else {
                    showValidationMessage('Please fill required fields: ' + miss.join(', '));
                }
            }
        }

        nextButtons.forEach(btn => {
            btn.addEventListener("click", function () {
                if (!validateStep(currentStep)) {
                    return;
                }

                if (currentStep === 1) {
                    generateTravelers();
                }

                if (currentStep < steps.length - 1) {
                    currentStep++;
                    showStep(currentStep);
                }

                if (currentStep === 2) {
                    updateSummary();
                }

                if (!validationMessageEl.classList.contains('d-none')) {
                    const miss = getMissingRequiredAcrossVisitedSteps();
                    if (miss.length === 0) hideValidationMessage();
                    else showValidationMessage('Please fill required fields: ' + miss.join(', '));
                }
            });
        });

        prevButtons.forEach(btn => {
            btn.addEventListener("click", function () {
                if (currentStep > 0) {
                    currentStep--;
                    showStep(currentStep);
                }
            });
        });

        function forceNumericInput(input) {
            input.addEventListener("keydown", function (e) {
                const allowedKeys = ["Backspace", "Delete", "ArrowLeft", "ArrowRight", "Tab", "Home", "End"];

                if (allowedKeys.includes(e.key)) return;

                if (!/^[0-9]$/.test(e.key)) {
                    e.preventDefault();
                }
            });

            input.addEventListener("input", function () {
                this.value = this.value.replace(/\D/g, '');
            });
        }

        function isValidEmail(email) {
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
        }

        markRequiredFields();
        attachFieldListeners();
        showStep(currentStep);
    });
}());

