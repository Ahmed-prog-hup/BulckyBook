var dataTable;

$(document).ready(function ()
{
    loadDataTable();
});



function loadDataTable(){
    dataTable = $('#TableData').DataTable({
        "ajax": {url: '/admin/company/getall'},
            
        "columns": [
                { data: 'name',"Width":"25%" },
                { data: 'streetAddress', "Width": "15%" },
                { data: 'state', "Width": "10%" },
                { data: 'city', "Width": "15%" },
                { data: 'phoneNumber', "Width": "10%" },
               /* { data: 'id', "Width": "15%" },*/
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/admin/company/upsert?id=${data}" class= "btn btn-primary mx-2"><i class="bi bi-pencil-square"></i>Edit</a>
                    <a onClick=Delete('/admin/company/delete/${data}') class= "btn btn-danger mx-2"><i class="bi bi-trash-fill"></i>DELETE</a>
                    </div>`
                },
                "Width": "25%"
            }
            ]
      });
}

function Delete(url)
{
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire(
                $.ajax({
                    url: url,
                    type:'DELETE',
                    success: function (data) {
                        dataTable.ajax.reload();
                        toastr.success(data.message)
                    }
                })
            )
        }
    })
}




