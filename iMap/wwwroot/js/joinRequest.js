function approveRoomJoinRequest(id){
    $.ajax({
        url: "/Member/ApproveRoomJoinRequest",
        async:false,
        data:{id:id},
        type:"POST",
        success: function(success) {
            document.getElementById(`join-request-${id}`).remove();
        },
        error: function (request, status, error) {
            console.log(request.responseText);
        }
    });
}
function declineRoomJoinRequest(id){
    $.ajax({
        url: "/Member/DeclineRoomJoinRequest",
        async:false,
        data:{id:id},
        type:"POST",
        success: function(success) {
            document.getElementById(`join-request-${id}`).remove();
        },
        error: function (request, status, error) {
            console.log(request.responseText);
        }
    });
}