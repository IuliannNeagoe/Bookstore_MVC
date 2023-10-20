var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/admin/product/getall' },
        columns: [
            { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "15%" },
            { data: 'author', "width": "15%" },
            { data: 'category.name', "width": "10%" },
            { data: 'price', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                                <a href="/admin/product/upsert?myId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit </a>
                                <a onClick=Delete('/admin/product/delete?id=${data}') class="btn btn-danger mx-2"> <i class="bi bi-trash"></i> Delete </a>
                            </div>`
                },
                "width": "25%"
            }
        ]
    });
}

function Delete(url) {

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
            //if the confirm button is pressed, we send a DELETE request to the Controller
            $.ajax(
                {
                    url: url,
                    type: 'DELETE',
                    //this is what we get back from the controller. In this case, it will be:
                    //return Json(new { success = true, message = "Product deleted successfully!" }); -- this is stored in "data"
                    success: function (data) {
                        switch (data.success) {
                            case true:
                                toastr.success(data.message);
                                //reload the data, otherwise it wont get updated. - this sends the same request as the initialization
                                dataTable.ajax.reload();
                                break;
                            case false:
                                toastr.error(data.message);
                                break;
                        }
                    }

                }
            )
        }
    })
}
