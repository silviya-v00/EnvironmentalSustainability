$(document).ready(function () {
    let states = [];
    let cities = [];
    let isSearchBarsDisabled = false;

    // Needed to prevent too many requests
    const disableSearchBars = (duration, type) => {
        isSearchBarsDisabled = true;
        $('#searchInputCountry').prop('disabled', true);
        $('#searchInputState').prop('disabled', true);
        $('#searchInputCity').prop('disabled', true);
        $('#searchingData').css('display', '');
        setTimeout(() => {
            $('#searchingData').css('display', 'none');
            $('#searchInputCountry').prop('disabled', false);
            $('#searchInputState').prop('disabled', false);
            $('#searchInputCity').prop('disabled', false);
            isSearchBarsDisabled = false;
            updateCityDetailsVisibility(type)
        }, duration);
    };

    const updateCityDetailsVisibility = (type) => {
        if (type === 'city') {
            if (isSearchBarsDisabled) {
                $('#msgNoData').css('display', '');
                $('#air-and-weather-container').css('display', 'none');
            } else {
                $('#msgNoData').css('display', 'none');
                $('#air-and-weather-container').css('display', '');
            }
        }
    };

    $(document.body).on('click', '.searchInput', function (event) {
        event.stopPropagation();

        $(this).trigger('input');
        $('.search-results').css('display', 'none');
        const resultsId = `#searchResults${$(this).attr('id').replace('searchInput', '')}`;
        $(resultsId).css('display', 'block');
    });

    $(document.body).on('click', function () {
        $('.search-results').css('display', 'none');
    });

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
                disableSearchBars(10000, type);
                fetchStates(name);
            } else if (type === 'state') {
                $('#searchInputCity').val('');
                $('#msgNoData').css('display', 'block');
                $('#air-and-weather-container').css('display', 'none');
                disableSearchBars(10000, type);
                fetchCities($('#searchInputCountry').val(), name);
            } else if (type === 'city') {
                disableSearchBars(10000, type);
                fetchCityData(name, $('#searchInputState').val(), $('#searchInputCountry').val());
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

        const fetchCityData = (city, state, country) => {
            $.get(`${apiUrlAirVisual}/city?city=${city}&state=${state}&country=${country}&key=${apiKeyAirVisual}`, function (data) {
                if (data.status === "success") {
                    const { pollution, weather } = data.data.current;
                    const aqi = pollution && pollution.aqius ? pollution.aqius : null;

                    if (aqi !== null) {
                        const { tp, hu, ws, pr } = weather;
                        updateAQICard(aqi);
                        $('#wTemperature').text(`${tp}°C`);
                        $('#wHumidity').text(`${hu}%`);
                        $('#wWind').text(`${(ws * 3.6).toFixed(2)} km/h`);
                        $('#wPressure').text(`${pr} mbar`);
                    } else {
                        $('#msgNoData').css('display', '');
                        $('#air-and-weather-container').css('display', 'none');
                    }
                } else {
                    console.error("Failed to fetch city data.");
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
