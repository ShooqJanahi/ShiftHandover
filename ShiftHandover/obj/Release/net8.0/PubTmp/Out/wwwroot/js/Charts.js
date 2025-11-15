document.addEventListener('DOMContentLoaded', function () {
    // Load Google Charts
    google.charts.load('current', { packages: ['corechart'] });
    google.charts.setOnLoadCallback(drawCharts);

    function drawCharts() {
        drawLineChart(); // Call separately
        drawDonutChart();
    }

    function drawDonutChart() {
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
    }

    function drawLineChart() {
        if (document.getElementById('curve_chart')) {
            let groupBy = document.getElementById('timeFilter').value; // week or month

            let groupedData = {};

            for (let i = 0; i < window.dashboardData.shiftDates.length; i++) {
                let date = new Date(window.dashboardData.shiftDates[i]);

                let key;
                if (groupBy === "week") {
                    let onejan = new Date(date.getFullYear(), 0, 1);
                    let week = Math.ceil((((date - onejan) / 86400000) + onejan.getDay() + 1) / 7);

                    // Find first day of the week
                    let weekStart = new Date(date.setDate(date.getDate() - date.getDay()));
                    let weekEnd = new Date(date.setDate(weekStart.getDate() + 6));

                    let weekStartStr = `${weekStart.toLocaleString('default', { month: 'short' })} ${weekStart.getDate()}`;
                    let weekEndStr = `${weekEnd.toLocaleString('default', { month: 'short' })} ${weekEnd.getDate()}`;

                    key = `${weekStartStr} - ${weekEndStr}, ${weekStart.getFullYear()}`;

                } else {
                    // Group by Year-Month
                    key = `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}`;
                }

                if (!groupedData[key]) {
                    groupedData[key] = 0;
                }
                groupedData[key] += window.dashboardData.shiftCounts[i];
            }

            // Prepare chart data
            let lineDataArray = [['Period', 'Shifts']];
            for (let period in groupedData) {
                lineDataArray.push([period, groupedData[period]]);
            }

            var lineData = google.visualization.arrayToDataTable(lineDataArray);

            var lineOptions = {
                title: `Shifts Over Time (${groupBy})`,
                curveType: 'function',
                legend: { position: 'bottom' }
            };

            var lineChart = new google.visualization.LineChart(document.getElementById('curve_chart'));
            lineChart.draw(lineData, lineOptions);
        }
    }

    // When filter changes, redraw
    const timeFilterElement = document.getElementById('timeFilter');
    if (timeFilterElement) {
        timeFilterElement.addEventListener('change', function () {
            drawLineChart();
        });
    }
});
