var dataTable;

$(document).ready(function ()
{
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else {
        if (url.includes("pending")) {
            loadDataTable("pending");
        }
        else {
            if (url.includes("completed")) {
                loadDataTable("completed");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
    
});

function loadDataTable(Status){
    dataTable = $('#TableData').DataTable({
        "ajax": { url: '/admin/order/getall?status=' + Status },
            
        "columns": [
                { data: 'id',"Width":"5%" },
                { data: 'name', "Width": "25%" },
                { data: 'phoneNumber', "Width": "20%" },
                { data: 'applicationUser.email', "Width": "15%" },
                { data: 'orderStatus', "Width": "10%" },
                { data: 'orderTotal', "Width": "10%" },
               /* { data: 'id', "Width": "15%" },*/
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/admin/order/details?orderId=${data}" class= "btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                    </div>`
                },
                "Width": "25%"
            }
            ]
      });
}


