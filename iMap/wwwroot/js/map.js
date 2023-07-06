if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(success);
        } else {
            alert("Geo Location is not supported on your current browser!");
        }
        function generateGUID() {
            var d = new Date().getTime();
            var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = (d + Math.random() * 16) % 16 | 0;
                d = Math.floor(d / 16);
                return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
            });
            return uuid;
        }
        function success(position) {
            var lat = position.coords.latitude;
            var long = position.coords.longitude;
            var myLatlng = new google.maps.LatLng(lat, long);
            var myOptions = {
                center: myLatlng,
                zoom: 12,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            var map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);
            const marker = new google.maps.Marker({
                position: myLatlng,
                map: map,
                customId: generateGUID() // random GUID value
            });

            marker.setMap(map);
            marker.addListener('click', function () {
                alert('GUID value: ' + this.customId);
            });
            marker.addListener('click', function () {
                window.open('ChatPopupView', 'Popup0', 'width =350,height=450');
            });
        }