$(() => {
    $("#add-device").click(function () {
        $('#result').text('');

        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val()
        }

        $.post("/home/addDevice", sendingData, function (data) {
            var obj = JSON.parse(data);
            console.log(obj);
            if (obj['status']['code'] != '200' && obj['status']['code'] != '0') {
                if (obj['status']['message']) {
                    $("#result").text(obj['status']['message']);
                } else {
                    $("#result").text(obj['status']['description']);
                }
            } else if (obj['status']['code'] == '0') {
                $("#result").text(obj['status']['description']);
            } else {
                $("#result").text(data);
            }
        });
    });

    $("#delete-device").click(function () {
        $('#result').text('');

        let sendingData = {
            serial: $('#serial').val()
        }

        $.post("/home/deleteDevice", sendingData, function (data) {
            var obj = JSON.parse(data);
            console.log(obj);
            if (obj['status']['code'] != '200' && obj['status']['code'] != '0') {
                if (obj['status']['message']) {
                    $("#result").text(obj['status']['message']);
                } else {
                    $("#result").text(obj['status']['description']);
                }
            } else if (obj['status']['code'] == '0') {
                $("#result").text(obj['status']['description']);
            } else {
                $("#result").text(data);
            }
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
            var obj = JSON.parse(data);
            console.log(obj);
            if (obj['status']['code'] != '200' && obj['status']['code'] != '0') {
                $("#result").text(obj['status']['message']);
            } else if (obj['status']['code'] == '0') {
                $("#result").text(obj['status']['description']);
            } else {
                $("#result").text(data);
            }
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
            var obj = JSON.parse(data);
            console.log(obj);
            if (obj['status']['code'] != '200' && obj['status']['code'] != '0') {
                if (obj['status']['message']) {
                    $("#result").text(obj['status']['message']);
                } else {
                    $("#result").text(obj['status']['description']);
                }
            } else if (obj['status']['code'] == '0') {
                $("#result").text(obj['status']['description']);
            } else {
                $("#result").text(data);
            }
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
            var obj = JSON.parse(data);
            console.log(obj);
            if (obj['status']['code'] != '200' && obj['status']['code'] != '0') {
                if (obj['status']['message']) {
                    $("#result").text(obj['status']['message']);
                } else {
                    $("#result").text(obj['status']['description']);
                }
            } else if (obj['status']['code'] == '0') {
                $("#result").text(obj['status']['description']);
            } else {
                $("#result").text(data);
            }
        });
    });
})