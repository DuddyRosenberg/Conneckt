$(() => {
    $("#add-device").on('click', () => {
        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val()
        }

        $.post("/home/addDevice", { serial }, function (data) {
            $("#result").text(data);
        });
    });

    $("#activate").on('click', () => {
        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val(),
            zip: $('#zip').val()
        }

        $.post("/home/activate", sendingData, function (data) {
            $("#result").text(data);
        });
    });

    $("#external-port").on('click', () => {
        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val(),
            zip: $('#zip').val(),
            currentMIN: $('#current-min').val(),
            currentServiceProvider: $('#current-service-provider').val(),
            currentAccountNumber: $('#current-account-number').val(),
            CurrentVKey: $('#password').val(),
        }
        $.post("/home/externalPort", sendingData, function (data) {
            $("#result").text(data);
        });
    });
})