function initMap() {
    const map = new google.maps.Map(document.getElementById('map'), {
      center: { lat: 49.8397, lng: 24.0297 },
      zoom: 12
    });
  
    const mainBuilding = {
      position: { lat: 49.8397, lng: 24.0297 },
      type: 'Main'
    };
  
    const buildings = [
      { position: { lat: 49.8497, lng: 24.0397 }, type: 'Healthcare' },
      { position: { lat: 49.8297, lng: 24.0197 }, type: 'Industrial' },
      { position: { lat: 49.8197, lng: 24.0097 }, type: 'Office' },
      { position: { lat: 49.8097, lng: 23.9997 }, type: 'Multi-family' }
    ];
  
    const mainMarker = new google.maps.Marker({
      position: mainBuilding.position,
      map: map,
      icon: 'http://maps.google.com/mapfiles/ms/icons/red-dot.png'
    });
  
    const markers = buildings.map(building => {
      return new google.maps.Marker({
        position: building.position,
        map: map,
        icon: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png',
        buildingType: building.type
      });
    });
  
    const filterPanel = document.createElement('div');
    filterPanel.classList.add('filter-panel');
    map.controls[google.maps.ControlPosition.TOP_RIGHT].push(filterPanel);
  
    const buildingTypes = ['Healthcare', 'Industrial', 'Office', 'Multi-family'];
  
    buildingTypes.forEach(type => {
      const checkbox = document.createElement('input');
      checkbox.type = 'checkbox';
      checkbox.id = type;
      checkbox.checked = true;
  
      const label = document.createElement('label');
      label.htmlFor = type;
      label.textContent = type;
  
      checkbox.addEventListener('change', () => {
        markers.forEach(marker => {
          if (marker.buildingType === type) {
            marker.setMap(checkbox.checked ? map : null);
          }
        });
      });
  
      filterPanel.appendChild(checkbox);
      filterPanel.appendChild(label);
    });
  }
  