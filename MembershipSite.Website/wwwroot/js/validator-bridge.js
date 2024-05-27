// Disables form submissions if there are invalid fields.
// Fetch all the forms we want to apply custom Bootstrap validation styles to.
const forms = document.querySelectorAll('.needs-validation');
// Loop over them and prevent submission
Array.prototype.slice.call(forms)
    .forEach(function (form) {
    form.addEventListener('submit', function (event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }
        form.classList.add('was-validated');
    }, false);
});
const v = new aspnetValidation.ValidationService();
v.ValidationInputCssClassName = 'is-invalid'; // change from default of 'input-validation-error'
v.ValidationInputValidCssClassName = 'is-valid'; // change from default of 'input-validation-valid'
v.ValidationMessageCssClassName = 'invalid-feedback'; // change from default of 'field-validation-error'
v.ValidationMessageValidCssClassName = 'valid-feedback'; // change from default of 'field-validation-valid'
//v.ValidationSummaryCssClassName      = 'validation-summary-errors';  // unnecessary: bootstrap lacks validation summary component
//v.ValidationSummaryValidCssClassName = 'validation-summary-valid';   // "
v.bootstrap();
//# sourceMappingURL=validator-bridge.js.map