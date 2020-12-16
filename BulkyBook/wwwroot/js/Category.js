﻿var dataTable;

$(
    function () {
        loadDataTable();
    }
)


function loadDataTable() {
    dataTable = $("#tblData").dataTable({
        "ajax": {
            "url": "/Admin/Category/GetAll"
        },
        "colums": [
            { "data": "name", "width": "60%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                           <div class="text-center">
                             <a href="/Admin/Category/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                <i class="fas fa-edit"></i>
                            </a>
                             <a class="btn btn-danger text-white" style="cursor:pointer"><i class="fas fa-trash-alt"></i></a>
                           </div>             
                        `;
                },"width":"60%"
            }
        ]
    })
}