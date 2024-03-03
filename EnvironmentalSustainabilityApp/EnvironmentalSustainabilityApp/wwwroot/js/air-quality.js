$(document).ready(function () {
    let states = [];
    let cities = [];

    // Needed to prevent too many requests
    const disableSearchBars = (duration) => {
        $('#searchInputCountry').prop('disabled', true);
        $('#searchInputState').prop('disabled', true);
        $('#searchInputCity').prop('disabled', true);
        $('#searchingData').css('display', '');
        setTimeout(() => {
            $('#searchingData').css('display', 'none');
            $('#searchInputCountry').prop('disabled', false);
            $('#searchInputState').prop('disabled', false);
            $('#searchInputCity').prop('disabled', false);
        }, duration);
    };

    const fetchCountries = () => {
        $.get(`${apiUrlAirVisual}/countries?key=${apiKeyAirVisual}`, function (data) {
            if (data.status === "success") {
                const countriesData = data.data;
                const countries = countriesData.map((country, index) => {
                    return { id: index + 1, name: country.country };
                });
                initializeSearch(countries);
            } else {
                console.error("Failed to fetch countries.");
            }
        });
    };

    const initializeSearch = (countries) => {
        function searchLocations(type) {
            const inputId = `#searchInput${type.charAt(0).toUpperCase() + type.slice(1)}`;
            const resultsId = `#searchResults${type.charAt(0).toUpperCase() + type.slice(1)}`;
            const searchInput = $(inputId).val().toLowerCase();
            const searchResults = $(resultsId);
            searchResults.html('');

            let locations;
            switch (type) {
                case 'country':
                    locations = countries;
                    break;
                case 'state':
                    locations = states;
                    break;
                case 'city':
                    locations = cities;
                    break;
                default:
                    locations = [];
            }

            const filteredLocations = locations.filter(location => location.name.toLowerCase().includes(searchInput));

            const ul = $('<ul></ul>');
            filteredLocations.forEach(location => {
                const li = $('<li></li>');
                li.text(location.name);
                li.attr('data-id', location.id);
                li.click(() => selectLocation(location.id, location.name, type));
                ul.append(li);
            });

            searchResults.append(ul);

            if (filteredLocations.length > 0) {
                searchResults.css('display', 'block');
                searchResults.css('margin', '0px');
            } else {
                searchResults.css('display', 'none');
            }
        }

        function selectLocation(id, name, type) {
            $(`#searchInput${type.charAt(0).toUpperCase() + type.slice(1)}`).val(name);
            $(`#searchResults${type.charAt(0).toUpperCase() + type.slice(1)}`).html('');
            $(`#searchResults${type.charAt(0).toUpperCase() + type.slice(1)}`).css('display', 'none');

            if (type === 'country') {
                $('#searchInputState').val('');
                $('#searchInputCity').val('');
                $('#msgNoData').css('display', 'block');
                $('#air-and-weather-container').css('display', 'none');
                disableSearchBars(10000);
                fetchStates(name);
            } else if (type === 'state') {
                $('#searchInputCity').val('');
                $('#msgNoData').css('display', 'block');
                $('#air-and-weather-container').css('display', 'none');
                disableSearchBars(10000);
                fetchCities($('#searchInputCountry').val(), name);
            }

            if (type === 'city') {
                disableSearchBars(10000);
                $('#msgNoData').css('display', 'none');
                $('#air-and-weather-container').css('display', '');

                const aqi = 2300;
                updateAQICard(aqi);
            }
        }

        const fetchStates = (country) => {
            $.get(`${apiUrlAirVisual}/states?country=${country}&key=${apiKeyAirVisual}`, function (data) {
                if (data.status === "success") {
                    const statesData = data.data;
                    states = statesData.map((state, index) => ({ id: index + 1, name: state.state }));
                } else {
                    console.error("Failed to fetch states.");
                }
            });
        };

        const fetchCities = (country, state) => {
            $.get(`${apiUrlAirVisual}/cities?state=${state}&country=${country}&key=${apiKeyAirVisual}`, function (data) {
                if (data.status === "success") {
                    const citiesData = data.data;
                    cities = citiesData.map((city, index) => ({ id: index + 1, name: city.city }));
                } else {
                    console.error("Failed to fetch cities.");
                }
            });
        };

        $('#searchInputCountry').on('input', () => searchLocations('country'));
        $('#searchInputState').on('input', () => searchLocations('state'));
        $('#searchInputCity').on('input', () => searchLocations('city'));

        $('#searchInputState').prop('disabled', true);
        $('#searchInputCity').prop('disabled', true);
    };

    fetchCountries();

    function updateAQICard(aqi) {
        const aqIndex = $('#aqIndex');
        const aqType = $('#aqType');
        const aqDetails = $('.aqDetails');

        let bgColor, fontColor, type;
        if (aqi >= 0 && aqi <= 50) {
            bgColor = '#00e400'; // Good
            fontColor = '#000000'; // Black
            type = 'Good';
        } else if (aqi >= 51 && aqi <= 100) {
            bgColor = '#ffff00'; // Moderate
            fontColor = '#000000'; // Black
            type = 'Moderate';
        } else if (aqi >= 101 && aqi <= 150) {
            bgColor = '#ff7e00'; // Unhealthy for Sensitive Groups
            fontColor = '#000000'; // Black
            type = 'Unhealthy for Sensitive Groups';
        } else if (aqi >= 151 && aqi <= 200) {
            bgColor = '#ff0000'; // Unhealthy
            fontColor = '#ffffff'; // White
            type = 'Unhealthy';
        } else if (aqi >= 201 && aqi <= 300) {
            bgColor = '#8f3f97'; // Very Unhealthy
            fontColor = '#ffffff'; // White
            type = 'Very Unhealthy';
        } else {
            bgColor = '#7e0023'; // Hazardous
            fontColor = '#ffffff'; // White
            type = 'Hazardous';
        }

        aqIndex.text(aqi);
        aqType.text(type);
        aqDetails.css('background-color', bgColor);
        aqDetails.css('color', fontColor);
    }
});
