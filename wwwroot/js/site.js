
// alerts.js

// Standard alert for Save action
function showSaveAlert()
{
    Swal.fire({
        icon: 'success',
        title: 'Data Saved',
        text: 'Your data has been saved successfully.',
        showConfirmButton: true,
    });
}

// Standard alert for Submit action
function showSubmitAlert()
{
    Swal.fire({
        icon: 'success',
        title: 'Data Submitted',
        text: 'Your submission was successful.',
        showConfirmButton: true,
    });
}

// Standard alert for Delete action
function showDeleteAlert()
{
    Swal.fire({
        icon: 'warning',
        title: 'Data Deleted',
        text: 'The data has been deleted successfully.',
        showConfirmButton: true,
    });
}

// Standard alert for Approve action
function showApproveAlert()
{
    Swal.fire({
        icon: 'success',
        title: 'Approved',
        text: 'The action has been approved successfully.',
        showConfirmButton: true,
    });
}

// Standard alert for Reject action
function showRejectAlert()
{
    Swal.fire({
        icon: 'error',
        title: 'Rejected',
        text: 'The action has been rejected.',
        showConfirmButton: true,
    });
}

// Standard alert for Error action
function showErrorAlert(message)
{
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message || 'An error occurred. Please try again later.',
        showConfirmButton: true,
    });
}

// code for spinners
function showSpinner()
{
    // show the spinner container
    $("#spinner-container").removeClass("d-none");
}

function hideSpinner()
{
    // Hide the spinner container
    $("#spinner-container").addClass("d-none");
}

// code to prevent browswer back button
    //function preventBack() {
    //    window.history.forward();
    //}
    //setTimeout("preventBack()", 0);
    //window.onunload = function () {
    //    null
    //};

// code to change normal ddl to select2 ddl
$(document).ready(function ()
{
    $('.myselect').select2({
        theme: "bootstrap-5",
        width: function ()
        {
            return $(this).data('width') ? $(this).data('width') : $(this).hasClass('w-100') ? '100%' : 'style';
        },
        placeholder: "Select..."
    });
});
