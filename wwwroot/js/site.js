(() => {
  // Keep keyboard users oriented after a form validation round-trip.
  const firstError = document.querySelector('.field-validation:not(:empty)');
  if (firstError) {
    const field = firstError.closest('.field')?.querySelector('input, select, textarea');
    field?.focus();
  }
})();
