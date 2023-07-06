let signalRConnected = false;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start().then(function () {
            console.log('SignalR Started...')
            signalRConnected = true;
            viewModel.roomList();
            viewModel.userList();
        });
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

start();

async function WaitForHubConnection() {
    while (true) {
        if (signalRConnected) {
            return;
        } else {
            await new Promise(resolve => setTimeout(resolve, 200));
        }
    }
}

$(document).ready(async function () {
    await WaitForHubConnection();
});

connection.on("newMessage", function (messageView) {
    //mesaj private mı diye kontrol ediyoruz
    if (messageView.isPrivate) {
        let url = window.location.href;
        if (url.toLowerCase().includes("chatpopupdetailview")) {
            //eğer private ise ve özel chat alanı açıksa username ve roomid parametrelerini scrap ediyoruz
            let checkUserName = url.split("username=")[1];
            let roomId = null;
            if (checkUserName.includes("&")){
                let splittedCheckUserName = checkUserName.split("&");
                for(let i=0;i<splittedCheckUserName.length;i++){
                    if (!splittedCheckUserName[i].includes("roomId")){
                        checkUserName=splittedCheckUserName[i];
                    }else{
                        roomId = splittedCheckUserName[1].split("roomId=")[1];
                    }
                }
            }
            // chat mesajını basıyoruz
            let isMine = messageView.fromUserName === viewModel.myProfile().userName();
            let message = new ChatMessage(messageView.id, messageView.content, messageView.timestamp, messageView.fromUserName, isMine);
            viewModel.chatMessages.push(message);
            $(`#messages-container-${checkUserName}-${roomId}`).animate({scrollTop: $(`#messages-container-${checkUserName}-${roomId}`)[0].scrollHeight}, 1000);
        }else{
            //eğer private mesaj ise ama chat alanı açık değilse alert yolluyoruz
            alert(`New message from ${messageView.fromUserName}!`)
        }

    } else {
        //normal mesajsa direkt yolluyoruz
        let isMine = messageView.fromUserName === viewModel.myProfile().userName();
        let message = new ChatMessage(messageView.id, messageView.content, messageView.timestamp, messageView.fromUserName, isMine);
        viewModel.chatMessages.push(message);
        $(".messages-container").animate({scrollTop: $(".messages-container")[0].scrollHeight}, 1000);
    }
});

connection.on("getProfileInfo", function (user) {
    viewModel.myProfile(new ProfileInfo(user.userName));
    viewModel.isLoading(false);
});

connection.on("addUser", function (user) {
    viewModel.userAdded(new ChatUser(user.userName, user.currentRoom, user.device));
});

connection.on("removeUser", function (user) {
    viewModel.userRemoved(user.userName);
});

connection.on("addChatRoom", function (room) {
    viewModel.roomAdded(new ChatRoom(room.id, room.name, room.admin));
    addMarkerToMap(room.latitude, room.longitude, room.id, room.image);
});

connection.on("updateChatRoom", function (room) {
    viewModel.roomUpdated(new ChatRoom(room.id, room.name, room.admin));
});

connection.on("removeChatRoom", function (id) {
    viewModel.roomDeleted(id);
    deleteMarkerFromMap(id);
});

connection.on("removeChatMessage", function (id) {
    viewModel.messageDeleted(id);
});

connection.on("onError", function (message) {
    viewModel.serverInfoMessage(message);
    $("#errorAlert").removeClass("d-none").show().delay(5000).fadeOut(500);
});

connection.on("onRoomDeleted", function () {
    if (viewModel.chatRooms().length == 0) {
        viewModel.joinedRoom(null);
    } else {
        viewModel.joinRoom(viewModel.chatRooms()[0]);
    }
});

connection.on("BanUser", function (roomId, userName) {
    if (viewModel.myProfile()?.userName() == userName) {
        joinRoom(roomId)
    }
});

function AppViewModel() {
    var self = this;

    self.message = ko.observable("");
    self.chatRooms = ko.observableArray([]);
    self.chatUsers = ko.observableArray([]);
    self.chatMessages = ko.observableArray([]);
    self.joinedRoom = ko.observable();
    self.serverInfoMessage = ko.observable("");
    self.myProfile = ko.observable();
    self.isLoading = ko.observable(true);


    self.showRoomActions = ko.computed(function () {
        return self.joinedRoom()?.admin() == self.myProfile()?.userName();
    });

    self.onEnter = function (d, e) {
        if (e.keyCode === 13) {
            self.sendNewMessage();
        }
        return true;
    }
    self.filter = ko.observable("");
    self.filteredChatUsers = ko.computed(function () {
        if (!self.filter()) {
            return self.chatUsers();
        } else {
            return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                var userName = user.userName().toLowerCase();
                return userName.includes(self.filter().toLowerCase());
            });
        }
    });

    self.sendNewMessage = function () {
        var text = self.message();
        if (text.startsWith("/")) {
            var receiver = text.substring(text.indexOf("(") + 1, text.indexOf(")"));
            var message = text.substring(text.indexOf(")") + 1, text.length);
            self.sendPrivate(receiver, message);
        } else {
            self.sendToRoom(self.joinedRoom(), self.message());
        }

        self.message("");
    }

    self.sendToRoom = function (room, message) {
        if (room != null) {
            if (room.name()?.length > 0 && message.length > 0) {
                let url = window.location.href;
                //eğer özel mesaj yollanıyorsa kontrolü yapıyoruz eğer özel mesajsa username ve roomidyi scraplıyoruz urlden
                if (url.toLowerCase().includes("chatpopupdetailview")) {
                    let checkUserName = url.split("username=")[1];
                    let roomId = null;
                    if (checkUserName.includes("&")){
                        let splittedCheckUserName = checkUserName.split("&");
                        for(let i=0;i<splittedCheckUserName.length;i++){
                            if (!splittedCheckUserName[i].includes("roomId")){
                                checkUserName=splittedCheckUserName[i];
                            }else{
                                roomId = splittedCheckUserName[1].split("roomId=")[1];
                            }
                        }
                    }
                    //private mesaj oluşturuyoruz
                    fetch('/api/Messages/CreatePrivate', {
                        method: 'POST',
                        headers: {'Content-Type': 'application/json'},
                        body: JSON.stringify({room: room.name(), content: message, toUserName: checkUserName})
                    });
                    self.sendPrivate(checkUserName, message);
                }else{
                    //normal mesaj
                    fetch('/api/Messages/Create', {
                        method: 'POST',
                        headers: {'Content-Type': 'application/json'},
                        body: JSON.stringify({room: room.name(), content: message, toUserName: null})
                    });  
                }
            }
        } else {
            //hiçbir senaryo yoksa diye özel mesaja düşen durumlar için aynı özel mesaj senaryosunu çalıştırıyor
            let url = window.location.href;
            if (url.toLowerCase().includes("chatpopupdetailview")) {
                let checkUserName = url.split("username=")[1];
                let roomId = null;
                if (checkUserName.includes("&")){
                    let splittedCheckUserName = checkUserName.split("&");
                    for(let i=0;i<splittedCheckUserName.length;i++){
                        if (!splittedCheckUserName[i].includes("roomId")){
                            checkUserName=splittedCheckUserName[i];
                        }else{
                            roomId = splittedCheckUserName[1].split("roomId=")[1];
                        }
                    }
                }
                fetch('/api/Messages/CreatePrivate', {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify({room: room.name(), content: message, toUserName: checkUserName})
                });
                self.sendPrivate(checkUserName, message);
            }
        }
    }

    self.sendPrivate = function (receiver, message) {
        if (receiver.length > 0 && message.length > 0) {
            connection.invoke("SendPrivate", receiver.trim(), message.trim(), true);
        }
    }

    self.joinRoom = function (room) {
        connection.invoke("Join", room.name()).then(function () {
            self.joinedRoom(room);
            self.userList();
            self.messageHistory();
        });
    }

    self.roomList = function () {
        fetch('/api/Rooms')
            .then(response => response.json())
            .then(data => {
                self.chatRooms.removeAll();
                for (var i = 0; i < data.length; i++) {
                    self.chatRooms.push(new ChatRoom(data[i].id, data[i].name, data[i].admin));
                }
            });
    }

    self.userList = function () {
        connection.invoke("GetUsers", self.joinedRoom()?.name()).then(function (result) {
            self.chatUsers.removeAll();
            for (var i = 0; i < result.length; i++) {
                self.chatUsers.push(new ChatUser(result[i].userName,
                    result[i].userName,
                    result[i].currentRoom,
                    result[i].device))
            }
        });
    }

    self.createRoom = function () {
        //gerekli dataları formdan alıyoruz
        let checkRoomIcon = document.querySelector('input[name="fixImage"]:checked');
        let roomIcon = "";
        if (checkRoomIcon!=null){
            roomIcon = checkRoomIcon.value;
        }
        let roomName = document.getElementById("name").value;
        let roomCustomIcon = document.getElementById("roomCustomIconHolder").value
        let isPublicCheck = document.getElementById("isPublic").value;
        $(document).ready(function () {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function (position) {
                    let latitude = position.coords.latitude;
                    let longitude = position.coords.longitude;
                    let isPublic;
                    if (isPublicCheck === 'on') {
                        isPublic = true;
                    } else {
                        isPublic = false;
                    }
                    //eğer kullanıcı hazır ikonlardan seçmişse aşağıdaki posttan eğer custom seçtiyse diğer posta yolluyoruz
                    if (roomCustomIcon != null && roomCustomIcon != "") {
                        fetch('/api/Rooms/Create', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({
                                name: roomName,
                                isPublic: isPublic,
                                latitude: latitude,
                                longitude: longitude,
                                image: roomCustomIcon
                            })
                        }).then(async (response) => {
                            if (response.status == 400) {
                                let responseJson = await response.json();
                                alert(responseJson.Message);
                            };
                        });
                    }else{
                        fetch('/api/Rooms/Create', {
                            method: 'POST',
                            headers: {'Content-Type': 'application/json'},
                            body: JSON.stringify({
                                name: roomName,
                                isPublic: isPublic,
                                latitude: latitude,
                                longitude: longitude,
                                image: roomIcon
                            })
                        }).then(async (response) => {
                            if (response.status == 400) {
                                let responseJson = await response.json();
                                alert(responseJson.Message);
                            };
                        });   
                    }
                });
            }
        });
    }

    self.editRoom = function () {
        var roomId = self.joinedRoom().id();
        var roomName = $("#newRoomName").val();
        let isPublicCheck = $('#isRoomPublic').val();
        let isPublic;
        if (isPublicCheck === 'on') {
            isPublic = true;
        } else {
            isPublic = false;
        }
        fetch('/api/Rooms/' + roomId, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id: roomId, name: roomName, isPublic: isPublic })
        });
    }

    self.deleteRoom = function () {
        fetch('/api/Rooms/' + self.joinedRoom().id(), {
            method: 'DELETE',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({id: self.joinedRoom().id()})
        });
    }

    self.deleteMessage = function () {
        var messageId = $("#itemToDelete").val();

        fetch('/api/Messages/' + messageId, {
            method: 'DELETE',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({id: messageId})
        });
    }

    self.messageHistory = function () {
        let url = window.location.href;
        // mesaj geçmişini çekerken özel mesaj senaryosundaki scrap işleminin aynısnı yapıyoruz eğer özel mesaj senaryosu varsa özel mesajlar bu senaryoya göre çekiliyor
        if (url.toLowerCase().includes("chatpopupdetailview")) {
            let checkUserName = url.split("username=")[1];
            let roomId = null;
            if (checkUserName.includes("&")){
                let splittedCheckUserName = checkUserName.split("&");
                for(let i=0;i<splittedCheckUserName.length;i++){
                    if (!splittedCheckUserName[i].includes("roomId")){
                        checkUserName=splittedCheckUserName[i];
                    }else{
                        roomId = splittedCheckUserName[1].split("roomId=")[1];
                    }
                }
            }
            if (checkUserName != "") {
                //özel mesajları çekiyoruz ve burada özel mesaj domuna basıyoruz
                fetch('/api/Messages/GetPrivateMessages?username=' +checkUserName)
                    .then(response => response.json())
                    .then(data => {
                        self.chatMessages.removeAll();
                        for (var i = 0; i < data.length; i++) {
                            var isMine = data[i].fromUserName == self.myProfile().userName();
                            self.chatMessages.push(new ChatMessage(data[i].id,
                                data[i].content,
                                data[i].timestamp,
                                data[i].fromUserName,
                                isMine))
                        }

                        $(`#messages-container-${checkUserName}-${roomId}`).animate({scrollTop: $(`#messages-container-${checkUserName}-${roomId}`)[0].scrollHeight}, 1000);
                    });
                return;
            }
        }
        else {
            //normal mesaj senaryosu
            fetch('/api/Messages/Room/' + viewModel.joinedRoom().name())
                .then(response => response.json())
                .then(data => {
                    self.chatMessages.removeAll();
                    for (var i = 0; i < data.length; i++) {
                        var isMine = data[i].fromUserName == self.myProfile().userName();
                        self.chatMessages.push(new ChatMessage(data[i].id,
                            data[i].content,
                            data[i].timestamp,
                            data[i].fromUserName,
                            isMine))
                    }

                    $(".messages-container").animate({scrollTop: $(".messages-container")[0].scrollHeight}, 1000);
                });
        }
    }

    self.roomAdded = function (room) {
        self.chatRooms.push(room);
    }

    self.roomUpdated = function (updatedRoom) {
        var room = ko.utils.arrayFirst(self.chatRooms(), function (item) {
            return updatedRoom.id() == item.id();
        });

        room.name(updatedRoom.name());

        if (self.joinedRoom().id() == room.id()) {
            self.joinRoom(room);
        }
    }

    self.roomDeleted = function (id) {
        var temp;
        ko.utils.arrayForEach(self.chatRooms(), function (room) {
            if (room.id() == id)
                temp = room;
        });
        self.chatRooms.remove(temp);
        window.close();
    }

    self.messageDeleted = function (id) {
        var temp;
        ko.utils.arrayForEach(self.chatMessages(), function (message) {
            if (message.id() == id)
                temp = message;
        });
        self.chatMessages.remove(temp);
    }

    self.userAdded = function (user) {
        self.chatUsers.push(user);
    }

    self.userRemoved = function (id) {
        var temp;
        ko.utils.arrayForEach(self.chatUsers(), function (user) {
            if (user.userName() == id)
                temp = user;
        });
        self.chatUsers.remove(temp);
    }

    self.uploadFiles = function () {
        var form = document.getElementById("uploadForm");
        $.ajax({
            type: "POST",
            url: '/api/Upload',
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function () {
                $("#UploadedFile").val("");
            },
            error: function (error) {
                alert('Error: ' + error.responseText);
            }
        });
    }
}

function ChatRoom(id, name, admin) {
    var self = this;
    self.id = ko.observable(id);
    self.name = ko.observable(name);
    self.admin = ko.observable(admin);
}

function ChatUser(userName, currentRoom, device) {
    var self = this;
    self.userName = ko.observable(userName);
    self.currentRoom = ko.observable(currentRoom);
    self.device = ko.observable(device);
}

function ChatMessage(id, content, timestamp, fromUserName, isMine) {
    var self = this;
    self.id = ko.observable(id);
    self.content = ko.observable(content);
    self.timestamp = ko.observable(timestamp);
    self.timestampRelative = ko.pureComputed(function () {
        // Get diff
        var date = new Date(self.timestamp());
        var now = new Date();
        var diff = Math.round((date.getTime() - now.getTime()) / (1000 * 3600 * 24));

        // Format date
        var {dateOnly, timeOnly} = formatDate(date);

        // Generate relative datetime
        if (diff == 0)
            return `Today, ${timeOnly}`;
        if (diff == -1)
            return `Yestrday, ${timeOnly}`;

        return `${dateOnly}`;
    });
    self.timestampFull = ko.pureComputed(function () {
        var date = new Date(self.timestamp());
        var {fullDateTime} = formatDate(date);
        return fullDateTime;
    });
    self.fromUserName = ko.observable(fromUserName);
    self.isMine = ko.observable(isMine);

}

function ProfileInfo(userName) {
    var self = this;
    self.userName = ko.observable(userName);
}

function formatDate(date) {
    // Get fields
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var d = hours >= 12 ? "PM" : "AM";

    // Correction
    if (hours > 12)
        hours = hours % 12;

    if (minutes < 10)
        minutes = "0" + minutes;

    // Result
    var dateOnly = `${day}/${month}/${year}`;
    var timeOnly = `${hours}:${minutes} ${d}`;
    var fullDateTime = `${dateOnly} ${timeOnly}`;

    return {dateOnly, timeOnly, fullDateTime};
}

var viewModel = new AppViewModel();
ko.applyBindings(viewModel);

function isAdmin(userName) {
    //kullanıcı odadaki adminmi kontrolünü yapıyoruz
    let checkAdmin = viewModel.showRoomActions();
    let roomAdmin = viewModel.joinedRoom()?.admin();
    if (userName() == roomAdmin) {
        return false;
    }
    return checkAdmin;
}

function joinRoom(roomId) {
    //signalr bağlantısı gerçekleşmiş mi diye bakıyoruz
    if (!signalRConnected) {
        window.setTimeout(joinRoom, 200, roomId);
    } else {
        let room;
        //kullanıcının odasını çekip oraya katılmasını sağlıyoruz eğer data -1 dönerse kullanıcı banlı anlamına geliyor o zaman kullanıcıyı banlandığını gösteriyoruz
        $.ajax({
            url: "/api/Rooms/" + roomId,
            async: false,
            success: function (data) {
                if (data.id == -1) {
                    let url = window.location.href;
                    if (url.includes("roomId=")) {
                        let checkRoomId = url.split("roomId=")[1];
                        if (checkRoomId == roomId.toString()) {
                            window.location.href = "/Member/ChatPopupViewBanned";
                            return;
                        }
                    }

                }
                room = data;
            },
            error: function (request, status, error) {
                console.log(request.responseText);
            }
        });
        if (room != null) {
            let chatRoom = new ChatRoom(room.id, room.name, room.admin);
            viewModel.joinRoom(chatRoom);
        }
    }
}

function banUser(element, roomId) {
    //kullanıcı banlıyoruz daha sonra onu banlanma ekranına yolluyoruz
    let userNameElement = element.parentElement.parentElement.parentElement;
    let userName = userNameElement.getAttribute("data-username");
    let bannedUser = {userName: userName, roomId: roomId};
    fetch('/api/Rooms/BanUser', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(bannedUser)
    });
}

function privateChat(elem) {
    //kullanıcının adına bastığımızda popup açtığımız senaryo
    let userName = elem.getAttribute('data-username')
    let roomId = document.getElementById("chat-id-private").value;
    connection.stop().then(function() {
        window.open(
            '/Member/ChatPopupDetailView?username=' + userName+"&roomId="+roomId,
            '_blank',
            'width=1200,height=800'
        );
    });
}
