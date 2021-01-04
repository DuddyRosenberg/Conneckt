$(document).ready(function () {
    var dataArr = [];
    console.log(data);
    console.log(JSON.stringify(data));
    $.each(data.customerAccount.products, function (index, value) {
        var arr = [value.productSerialNumber, value.status];
        if (value.supportingResources) {
            var result = value.supportingResources.filter(obj => {
                return obj.resourceCategory == 'SIM_CARD'
            });
            if (result) {
                arr.push(result[0].serialNumber);
            } else {
                arr.push('-');
            }
        } else {
            arr.push('-');
        }
        if (value.supportingResources) {
            var lineResult = value.supportingResources.filter(obj => {
                return obj.resourceCategory == 'LINE'
            });
            if (lineResult.length > 0) {
                arr.push(lineResult[0].serialNumber);
            } else {
                arr.push('-');
            }
        } else {
            arr.push('-');
        }
        if (value.relatedServices) {
            if (value.relatedServices[0].category == 'SERVICE_PLAN') {
                arr.push(value.relatedServices[0].characteristics[0].value);
                arr.push(value.relatedServices[0].validFor.endDate);
            } else {
                arr.push('-');
                arr.push('-');
            }
        } else {
            arr.push('-');
            arr.push('-');
        }
        dataArr.push(arr);
    });

    $('#my-table').DataTable({
        data: dataArr,
        pageLength: 30,
        columns: [
            { title: "Serial Number" },
            { title: "Status" },
            { title: "SIM Card" },
            { title: "MDN" },
            { title: "Plan" },
            { title: "End Date" }
        ]
    });
});