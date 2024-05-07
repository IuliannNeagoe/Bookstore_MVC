var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/admin/user/getall' },
        columns: [
            { data: 'name', "width": "25%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'company.name', "width": "10%" },
            { data: 'role', "width": "10%" },
            {
                data: {id: "id", lockoutEnd:"lockoutEnd"},
                "render": function (data) {
                    var now = new Date().getTime();
                    var banTime = new Date(data.lockoutEnd).getTime();

                    if (now < banTime) {
                        return `
                        <div class="text-center">
                               <a onClick=BanUnban('${data.id}') class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                                    <i class="bi bi-unlock-fill"></i> Unban
                               </a>
                        </div>`
                    }
                    else {
                        return `
                        <div class="text-center">
                               <a onClick=BanUnban('${data.id}') class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                                    <i class="bi bi-lock-fill"></i> Ban
                               </a>
                        </div>`
                    }
                   
                },
                "width": "25%"
            }
        ]
    });
}

function BanUnban(id) {
    var x = JSON.stringify(id)
    $.ajax(
        {
            type: "POST",
            url: '/admin/user/BanUnban',
            data: JSON.stringify(id),
            contentType: "application/json",
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message);
                    dataTable.ajax.reload();
                }
            }
        });
}

