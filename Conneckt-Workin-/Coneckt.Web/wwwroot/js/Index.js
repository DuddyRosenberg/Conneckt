$(() => {
    $("#execute-bulk").click(function () {
        $.post("/home/executebulk", function (data) {
            $("#result").text(data);
        })
    })

    $("#add-device").click(function () {
        $('#result').text('');

        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val()
        }

        $.post("/home/addDevice", sendingData, function (data) {
            $("#result").text(data);
        });
    });

    $("#delete-device").click(function () {
        $('#result').text('');

        let sendingData = {
            serial: $('#serial').val()
        }

        $.post("/home/deleteDevice", sendingData, function (data) {
            $("#result").text(data);
        });
    });

    $("#activate").on('click', () => {
        $('#result').text('');
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
        $('#result').text('');
        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val(),
            zip: $('#zip').val(),
            currentMIN: $('#current-min').val(),
            currentServiceProvider: $('#current-service-provider').val(),
            currentAccountNumber: $('#current-account-number').val(),
            CurrentVKey: $('#v-key').val(),
        }
        $.post("/home/externalPort", sendingData, function (data) {
            $("#result").text(data);
        });
    });

    $("#internal-port").on('click', () => {
        $('#result').text('');
        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val(),
            zip: $('#zip').val(),
            currentMIN: $('#current-min').val(),
            currentServiceProvider: $('#current-service-provider').val(),
            currentAccountNumber: $('#current-account-number').val(),
        }
        $.post("/home/internalPort", sendingData, function (data) {
            $("#result").text(data);
        });
    });

    $("#get-balance").on('click', () => {
        let phoneNumber = $("#phone-number").val();

        $.get('/home/getBalance', { phoneNumber }, (data) => {
            $("#result").text(data);
        })
    })
})