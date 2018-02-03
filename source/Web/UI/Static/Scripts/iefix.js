$(document).on('click', 'form button[type=submit]', function () {
    var $this = $(this);
    var action = $this.attr('formaction');
    if (action)
        $this.closest('form').attr('action', action);
});
