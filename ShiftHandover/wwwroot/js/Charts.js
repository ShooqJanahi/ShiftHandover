document.addEventListener('DOMContentLoaded', function () {
    // Load Google Charts
    google.charts.load('current', { packages: ['corechart'] });
    google.charts.setOnLoadCallback(drawCharts);

    function drawCharts() {
        // Donut Pie Chart
        if (document.getElementById('donutchart')) {
            var pieData = google.visualization.arrayToDataTable([
                ['Status', 'Count'],
                ['Claimed', window.dashboardData.claimed],
                ['Unclaimed', window.dashboardData.unclaimed]
            ]);

            var pieOptions = {
                title: 'Shift Claim Status',
                pieHole: 0.4,
            };

            var pieChart = new google.visualization.PieChart(document.getElementById('donutchart'));
            pieChart.draw(pieData, pieOptions);
        }

        // Line Chart
        if (document.getElementById('curve_chart')) {
            var lineDataArray = [['Date', 'Shifts']];
            for (let i = 0; i < window.dashboardData.shiftDates.length; i++) {
                lineDataArray.push([window.dashboardData.shiftDates[i], window.dashboardData.shiftCounts[i]]);
            }

            var lineData = google.visualization.arrayToDataTable(lineDataArray);

            var lineOptions = {
                title: 'Shifts Over Time',
                curveType: 'function',
                legend: { position: 'bottom' }
            };

            var lineChart = new google.visualization.LineChart(document.getElementById('curve_chart'));
            lineChart.draw(lineData, lineOptions);
        }
    }
});
