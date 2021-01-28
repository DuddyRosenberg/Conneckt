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
            sim: $('#activate-sim').val(),
            serial: $('#activate-serial').val(),
            zip: $('#activate-zip').val(),
            paymentMeanID: $('#activate-payment').val(),
            cvv: $('#activate-cvv').val(),
            billingAddress: {
                addressLine1: $('#activate-billing-address').val(),
                city: $('#activate-billing-city').val(),
                stateOrProvince: $('#activate-billing-state').val(),
                country: $('#activate-billing-country').val(),
                zipCode: $('#activate-billing-zip').val()
            }
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
            serial: $('#am-device').val(),
            useLine: $('#am-line').is(':checked')
        }

        $.post("/home/DeactivateAndRetaineDays", sendingData, function (data) {
            console.log(data);
            $("#result").text(data);
        });
    });

    $("#deactivate-past-due").click(function () {
        $('#result').text('');

        let sendingData = {
            serial: $('#am-device').val(),
            useLine: $('#am-line').is(':checked')
        }

        $.post("/home/DeactivatePastDue", sendingData, function (data) {
            console.log(data);
            $("#result").text(data);
        });
    });

    $("#reactivate").click(function () {
        $('#result').text('');

        let sendingData = {
            serial: $('#am-device').val(),
            zip: $('#am-zip-code').val(),
            useLine: $('#am-line').is(':checked')
        }

        $.post("/home/Reactivate", sendingData, function (data) {
            console.log(data);
            $("#result").text(data.status.message);
        });
    });

    $("#change-sim").on('click', () => {
        $('#result').text('');
        let sendingData = {
            sim: $('#sim').val(),
            serial: $('#serial').val(),
            zip: $('#zip').val(),
            currentMIN: $('#min').val()
        }

        $.post("/home/changeSIM", sendingData, function (data) {
            $("#result").text(data);
            console.log(data);
        });
    });

    $("#payment-details").on('click', () => {
        let paymentSource = {
            id: $('#payment-sources').val()
        }
        var html = '';
        $('#result').html('');

        $.post("/home/GetPaymentDetails", paymentSource, function (data) {
            console.log(data);
            if (data.status.code == "0") {

                html += '<h5>Card Details</h5>';
                html += '<table>';

                var cardDetails = data.response.paymentSourceDetail[0].cardDetails;
                for (var key in cardDetails) {
                    if (cardDetails.hasOwnProperty(key)) {
                        html += '<tr><td>' + key + ': </td><td>' + cardDetails[key] + '</td></tr>';
                    }
                }
                var custDetails = data.response.paymentSourceDetail[0].customerDetails;
                html += '<tr><td>Customer: </td><td>' + custDetails["firstName"] + ' ' + custDetails["lastName"] + '</td></tr>';


                html += '</table>';

                $("#result").append(html)
            } else {
                $("#result").text('Error getting Payment Details');
            }
        });
    });

    $("#device-details").on('click', () => {
        let sendingData = {
            id: $('#device-id').val(),
            type: $('input[name="device-type"]:checked').val()
        }
        var html = '';
        $('#result').html('');

        $.post("/home/GetDeviceDetails", sendingData, function (data) {
            console.log(data);

            if (data.status.code == "0") {

                html += '<h5>Device Details</h5>';
                html += '<table>';
                html += '<tr><td>Resource Category: </td><td>' + data.resource.physicalResource.resourceCategory + '</td></tr>';
                html += '<tr><td>Serial Number: </td><td>' + data.resource.physicalResource.serialNumber + '</td></tr>';
                html += '</table>';


                $.each(data.resource.physicalResource.supportingResources, function (index, res) {
                    html += '<h6>' + res.resourceCategory + '</h6>';

                    html += '<table>';
                    html += '<tr><td>Serial Number: </td><td>' + res.serialNumber + '</td></tr>';
                    html += '<tr><td>Status: </td><td>' + res.status + '</td></tr>';
                    html += '<tr><td>Type: </td><td>' + res.type + '</td></tr>';

                    if (res.hasOwnProperty('carrier')) {
                        if (res.carrier.hasOwnProperty('name')) html += '<tr><td>Carrier: </td><td>' + res.carrier.name + '</td></tr>';
                    }

                    html += '</table>';
                });

                $.each(data.resource.physicalResource.relatedServices, function (index, res) {
                    html += '<h6>' + res.category + '</h6>';

                    if (res.category == 'SERVICE_PLAN') {

                        html += '<table>';
                        html += '<tr><td>Name: </td><td>' + res.name + '</td></tr>';
                        html += '<tr><td>Subcategory: </td><td>' + res.subCategory + '</td></tr>';
                        html += '<tr><td>Valid Thru: </td><td>' + res.validFor.endDate + '</td></tr>';
                        html += '</table>';

                        html += '<table>';
                        $.each(res.characteristics, function (index, value) {
                            html += '<tr><td>' + value.name + ': </td><td>' + value.value + '</td></tr>';
                        });
                        html += '</table>';

                    } else if (res.category == 'DATA_ADD_ON') {
                        html += '<table>';
                        html += '<tr><td>Plan Data: </td><td>' + res.specifications.planData + '</td></tr>';
                        html += '</table>';
                    } else {
                        html += '<table>';
                        html += '<tr><td>Name: </td><td>' + res.name + '</td></tr>';
                        html += '</table>';
                    }
                });

                $("#result").append(html)
            } else {
                $("#result").text('Error getting Device Details');
            }
        });
    })
})