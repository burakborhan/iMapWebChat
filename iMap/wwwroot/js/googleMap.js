var markers = [];
var map;
function initMap() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            let latitude = position.coords.latitude;
            let longitude = position.coords.longitude;
            map = new google.maps.Map(document.getElementById('map'), {
                center: { lat: latitude, lng: longitude },
                zoom: 12,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            });
            let rooms;  
            //odaları çekiyoruz
            $.ajax({
                url: "/api/Rooms",
                async:false,
                success: function(data) {
                    rooms = data;
                },
                error: function (request, status, error) {
                    console.log(request.responseText);
                }
            });
            //odaları tek tek dolaşıp haritaya ve aynı zamanda bir arraye atıyoruz
            for (let i=0;i<rooms.length;i++){
                let imageToShow = rooms[i].image;
                //eğer market custom olarak yüklenmişse doğru yolu javascriptten bulabilmesi için prefix path ekliyoruz
                if (rooms[i].image.includes("uploaded-images")){
                    imageToShow="../../../"+rooms[i].image;
                }
                let roomMarker = new google.maps.Marker({
                    position: { lat: rooms[i].latitude, lng: rooms[i].longitude },
                    map: map,
                    icon:imageToShow,
                    url:`${rooms[i].id}`,
                    animation: google.maps.Animation.DROP,
                });
                //markerları hem mape hem de arraye basıyoruz
                markers.push(roomMarker);
                google.maps.event.addDomListener(roomMarker, 'click', function() {
                    window.open(
                        '/Member/ChatPopupView?roomId='+rooms[i].id,
                        '_blank',
                        'width=1200,height=800'
                    );
                });
                roomMarker.setMap(map);
            }
            // let marker = new google.maps.Marker({
            //     position: { lat: latitude, lng: longitude },
            //     map: map,
            // });
            // marker.setMap(map);
            // google.maps.event.addDomListener(marker, 'click', function() {
            //     window.open(
            //         'https://support.wwf.org.uk/earth_hour/index.php?type=individual',
            //         '_blank'
            //     );
            // });
        });
    }
}
$(document).ready(function () {
    initMap();
});

function addMarkerToMap(latitude,longitude,roomId,roomImage){
    //signalr ile datayı mape eklediğimiz senaryo burada da yukarıdaki mantıkla aynı çalışıyor
    let imageToShow = roomImage;
    if (roomImage.includes("uploaded-images")){
        imageToShow="../../../"+roomImage;
    }
    let roomMarker = new google.maps.Marker({
        position: { lat: latitude, lng: longitude },
        map: map,
        icon:imageToShow,
        url:`${roomId}`,
        animation: google.maps.Animation.DROP
    });
    google.maps.event.addDomListener(roomMarker, 'click', function() {
        window.open(
            '/Member/ChatPopupView?roomId='+roomId,
            '_blank',
            'width=1200,height=800'
        );
    });
    markers.push(roomMarker);
    roomMarker.setMap(map);
}

function deleteMarkerFromMap(id){
    //signalr ile markerı sildiğimiz senaryo statik olarak tuttuğumuz arrayden silip arrayi güncelliyoruz
    for(let i=0;i<markers.length;i++){
        if (markers[i].url==id){
            markers[i].setMap(null);
            markers = markers.filter(x=>x.url!=id);
        }
    }
}