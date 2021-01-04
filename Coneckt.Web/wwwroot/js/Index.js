$(() => {
    $("#execute-bulk").click(function () {
        $.post("/home/executebulk", function (data) {
            console.log(data);
            $("#result").text('Bulk execute complete. ' + data.length + ' lines executed. Check the Access document for responses.');
        })
    })

    $("#add-device").click(function () {
        $('#result').text('');

        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val()
        }

        $.post("/home/addDevice", sendingData, function (data) {
            console.log(data);
            if (data['request']) {
                $('#result').text(data['request'] + " - ");
                if (data['status']['description']) {
                    $('#result').append(data['status']['description']);
                } else {
                    $('#result').append(data['status']['message']);
                }
            } else {
                if (data['status']['description']) {
                    $('#result').text(data['status']['description']);
                } else {
                    $('#result').text(data['status']['message']);
                }
            }
        });
    });

    $("#delete-device").click(function () {
        $('#result').text('');

        let sendingData = {
            serial: $('#serial').val()
        }

        $.post("/home/deleteDevice", sendingData, function (data) {
            console.log(data);
            $('#result').text(data['status']['description']);
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
            console.log(data);
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
            console.log(data);
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
            console.log(data);
        });
    });

    $("#get-balance").on('click', (e) => {
        e.preventDefault();
        console.log('here');

        $('#result').text('');
        let phoneNumber = $("#phone-number").val();
        var html = '';

        $.get('/home/getBalance', { phoneNumber }, (data) => {
            console.log(data);
            console.log(JSON.stringify(data));
            if (data['status']['code'] == "0") {
                html = '<table>';
                html += '<tr><td>Balance Updated On </td><td>' + data['response']['configuration']['balanceUpdatedOn'] + '</td></tr>';
                var chars = data['response']['customerAccount'][0]['service']['products'][0]['relatedServices'][0]['serviceCharacteristics'];
                chars.forEach(function (item, index) {
                    html += '<tr><td>' + item['name'] + '</td><td>' + item['value'] + '</td></tr>';
                });
                html += '</table>';
                $('#result').append(html);
            }
        })
    });

    $("#deactivate-and-retaine-days").click(function () {
        $('#result').text('');

        let sendingData = {
            serial: $('#serial').val()
        }

        $.post("/home/DeactivateAndRetaineDays", sendingData, function (data) {
            $("#result").text(data);
        });
    });

    $("#deactivate-past-due").click(function () {
        $('#result').text('');

        let sendingData = {
            serial: $('#serial').val()
        }

        $.post("/home/DeactivatePastDue", sendingData, function (data) {
            $("#result").text(data);
        });
    });

    $("#change-sim").on('click', () => {
        $('#result').text('');
        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val(),
            zip: $('#zip').val()
        }

        $.post("/home/changeSIM", sendingData, function (data) {
            $("#result").text(data);
        });
    });
})