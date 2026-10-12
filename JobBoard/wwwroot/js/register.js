$(function () {
    function updateNameLabel() {
        var role = $('input[name="Role"]:checked').val();
        $('#nameLabel').text(role === 'Employer' ? 'Имя компании' : 'Имя пользователя');
    }

    $('input[name="Role"]').on('change', updateNameLabel);
    updateNameLabel();
});
