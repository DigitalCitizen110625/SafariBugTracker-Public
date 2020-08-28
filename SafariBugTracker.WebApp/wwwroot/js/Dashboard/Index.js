function InitCharts(model) {

    /*******************************
    *
    *  Selectors
    *
    *******************************/

    let categoryBreakdown = document.getElementById('lifelongCategoryBreakdown').getContext('2d');
    let statusBreakdown = document.getElementById('lifelongStatusBreakdown').getContext('2d');
    let monthlyProgressChart = document.getElementById('monthlyProgressChart').getContext('2d');



    /*******************************
    *
    *  Initializers
    *
    *******************************/


    //Colors are red, orange, yellow, green, teal, blue, purple, grey
    const colors = ["#FF6384", "#FF9F40", "#FFCD56", "#4BC086", "#4BC0C0", "#36A2EB", "#9966FF", "#C9CBCF" ];


    //Dynamically generate an array of colors for the category breakdown (one color per category)
    var i;
    let backgroundColors = [];
    for (i = 0; i < model.LifelongCategoryLables.length; i++)
    {
        let colorIndex = i;
        if (colorIndex > colors.length) {
            colorIndex = 0;
        }
        backgroundColors.push(colors[colorIndex]);
    }

    //Create the dataset used for the category breakdown chart
    let categoryBreakdownData = {
        datasets: [{
            backgroundColor: backgroundColors,
            data: model.LifelongCategoryValues
        }],
        labels: model.LifelongCategoryLables
    };


    //Repeat the steps from above, but for the resolve status breakdown chart
    i = 0;
    backgroundColors = [];
    for (i = 0; i < model.LifelongResolveStatusLabels.length; i++)
    {
        let colorIndex = i;
        if (colorIndex > colors.length) {
            colorIndex = 0;
        }
        backgroundColors.push(colors[colorIndex]);
    }

    let resolveStatusBreakdownData = {
        datasets: [{
            backgroundColor: backgroundColors,
            data: model.LifelongResolveStatusValues
        }],
        labels: model.LifelongResolveStatusLabels
    };


    //Create the datasets used for the monthly category breakdown stacked bar graph
    i = 0;
    var datasets = [];
    for (i = 0; i < model.MonthlyCategoryLabels.length; i++)
    {
        let colorIndex = i;
        if (colorIndex > colors.length) {
            colorIndex = 0;
        }

        //Each element (i.e. category) in the label array, corresponds to the element (i.e.  count) in the values array in the same position
        //Example: if label[0] is "Account", and values[0] is 12, then that means there were 12 issues submitted on that day, under the category of Accounts errors
        datasets.push({ label: model.MonthlyCategoryLabels[i], backgroundColor: colors[colorIndex], data: model.MonthlyCategoryValues[i] });
    }

    var barChartData = {
        labels: model.MonthlyChartXLabels,
        datasets: datasets
    };


    //Create the charts, and show the data
    let categoryChart = createDoughnutChart(categoryBreakdown, categoryBreakdownData);
    let resolveStatusChart= createDoughnutChart(statusBreakdown, resolveStatusBreakdownData);
    createStackedBarChart(monthlyProgressChart, barChartData);


    //The chart legends have been disabled in the doughnut charts, and a legend callback was added
    // We will use that to create a custom legend under the charts.
    $('#categoryChartLegend').prepend(categoryChart.generateLegend());
    $('#resolveStatusChartLegend').prepend(resolveStatusChart.generateLegend());


    /*******************************
    *
    *  Functions
    *
    *******************************/


    /*
    *   DESCRIPTION: Creates a doughnut chart for the specified element, using the passed in dataset
    */
    function createDoughnutChart(chartElement, data) {

        return new Chart(chartElement, {
            type: 'doughnut',
            data: data,
            options: {
                responsive: true,
                legend: {
                    display: false,
                    position: 'bottom',
                    align: 'start',
                    labels: {
                        fontColor: 'rgb(0, 0, 0)',   //Black font
                        fontSize: 10
                    }
                }, legendCallback: function (chart) {
                    var text = [];
                    text.push('<ul class="chart-legend-points">');
                    for (var i = 0; i < chart.data.datasets[0].data.length; i++) {
                        text.push('<li class="chart-legend-point"><div class="legendValue"><span style="background-color:' + chart.data.datasets[0].backgroundColor[i] + '">&nbsp;&nbsp;&nbsp;&nbsp;</span>');

                        if (chart.data.labels[i]) {
                            text.push('<span class="label">' + chart.data.labels[i] + '</span>');
                        }
                    }

                    text.push('</ul>');
                    return text.join('');
                }
            } 
        });
    }



    /*
    *   DESCRIPTION: Creates a stacked bar chart for the specified element, using the passed in dataset
    */
    function createStackedBarChart(element, data) {
        new Chart(element, {
            type: 'bar',
            data: data,
            options: {
                legend: {
                    display: true,
                    position: 'bottom',
                    align: 'start',
                    labels: {
                        fontColor: 'rgb(0, 0, 0)',   //Black font
                        fontSize: 11
                    }
                },scales: {
                    xAxes: [{
                        stacked: true
                    }],
                    yAxes: [{
                        stacked: true
                    }]
                }
            }
        });
    }



    /*
     *  DESCRIPTION : Generates a single random hexadecimal string that specifies a valid color 
     *   SOURCE     : https://css-tricks.com/snippets/javascript/random-hex-color/
     */
    function generateRandomColor() {
        const randomColor = Math.floor(Math.random() * 16777215).toString(16);
        var colorHexCode = "#" + randomColor;
        return colorHexCode.toString();
    }

}