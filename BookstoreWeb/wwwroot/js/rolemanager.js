$(document).ready(function () {

    $('#roleId').change(onRoleCheckboxChanged);   
    onRoleCheckboxChanged();
})


function onRoleCheckboxChanged() {

    var roleSelectElement = document.querySelector('#roleId')

    var selectedRole = roleSelectElement.options[roleSelectElement.selectedIndex].text;
    var companyTab = document.getElementById('companyId');

    if (selectedRole == "Company")
    {
        companyTab.style.display = "block";
    }
    else
    {
        companyTab.style.display = "none";
    }
}