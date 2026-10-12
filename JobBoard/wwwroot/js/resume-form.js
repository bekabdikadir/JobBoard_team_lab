$(function () {
    function workRow() {
        return '' +
            '<div class="card card-body mb-2 module-row"><div class="row g-2">' +
            '<div class="col-md-6"><input class="form-control" name="WorkExperiences[0].CompanyName" placeholder="Название компании" required maxlength="200" /></div>' +
            '<div class="col-md-6"><input class="form-control" name="WorkExperiences[0].Position" placeholder="Должность" required maxlength="200" /></div>' +
            '<div class="col-md-3"><input class="form-control" type="number" min="1950" max="2100" name="WorkExperiences[0].YearFrom" placeholder="Год начала" required /></div>' +
            '<div class="col-md-3"><input class="form-control" type="number" min="1950" max="2100" name="WorkExperiences[0].YearTo" placeholder="Год окончания" /></div>' +
            '<div class="col-12"><textarea class="form-control" rows="3" name="WorkExperiences[0].Responsibilities" placeholder="Обязанности" maxlength="4000"></textarea></div>' +
            '<div class="col-12 text-end"><button type="button" class="btn btn-sm btn-outline-danger remove-row">Удалить</button></div>' +
            '</div></div>';
    }

    function educationRow() {
        return '' +
            '<div class="card card-body mb-2 module-row"><div class="row g-2">' +
            '<div class="col-md-4"><select class="form-select" name="Educations[0].Kind">' +
            '<option value="Education">Образование</option>' +
            '<option value="Course">Курсы повышения квалификации</option></select></div>' +
            '<div class="col-md-8"><input class="form-control" name="Educations[0].Institution" placeholder="Учебное заведение / организатор курсов" required maxlength="250" /></div>' +
            '<div class="col-md-6"><input class="form-control" name="Educations[0].Specialty" placeholder="Специальность / название курса" maxlength="250" /></div>' +
            '<div class="col-md-3"><input class="form-control" type="number" min="1950" max="2100" name="Educations[0].YearFrom" placeholder="Год начала" /></div>' +
            '<div class="col-md-3"><input class="form-control" type="number" min="1950" max="2100" name="Educations[0].YearTo" placeholder="Год окончания" /></div>' +
            '<div class="col-12 text-end"><button type="button" class="btn btn-sm btn-outline-danger remove-row">Удалить</button></div>' +
            '</div></div>';
    }

    function reindex(containerSelector) {
        $(containerSelector).children('.module-row').each(function (index) {
            $(this).find('[name]').each(function () {
                var name = $(this).attr('name');
                $(this).attr('name', name.replace(/^(\w+)\[\d+\]/, '$1[' + index + ']'));
            });
        });
    }

    $('#addWork').on('click', function () {
        $('#workContainer').append(workRow());
        reindex('#workContainer');
    });

    $('#addEducation').on('click', function () {
        $('#educationContainer').append(educationRow());
        reindex('#educationContainer');
    });

    $('#resumeForm').on('click', '.remove-row', function () {
        var container = $(this).closest('.module-row').parent();
        $(this).closest('.module-row').remove();
        reindex('#' + container.attr('id'));
    });

    $('#resumeForm').on('submit', function () {
        reindex('#workContainer');
        reindex('#educationContainer');
    });
});
