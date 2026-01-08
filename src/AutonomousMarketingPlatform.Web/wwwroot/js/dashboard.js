$(document).ready(function () {
    loadDashboardMetrics();
    loadEmailPerformanceChart();
    loadSocialEngagementChart();
});

function loadDashboardMetrics() {
    $.get('/api/dashboard/metrics-summary', function (data) {
        $('#kpiEmailsSent').text(data.totalEmailsSent);
        $('#kpiEmailsOpened').text(data.totalEmailsOpened);
        $('#kpiClicks').text(data.totalClicks);
        $('#kpiSocialImpressions').text(data.totalSocialImpressions);
    });
}

function loadEmailPerformanceChart() {
    $.get('/api/dashboard/email-performance-chart-data', function (data) {
        const ctx = document.getElementById('emailPerformanceChart').getContext('2d');
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.map(item => item.date),
                datasets: [
                    {
                        label: 'Emails Sent',
                        data: data.map(item => item.emailsSent),
                        borderColor: 'rgba(255, 99, 132, 1)',
                        backgroundColor: 'rgba(255, 99, 132, 0.2)',
                        fill: true,
                    },
                    {
                        label: 'Emails Opened',
                        data: data.map(item => item.emailsOpened),
                        borderColor: 'rgba(54, 162, 235, 1)',
                        backgroundColor: 'rgba(54, 162, 235, 0.2)',
                        fill: true,
                    },
                    {
                        label: 'Clicks',
                        data: data.map(item => item.clicks),
                        borderColor: 'rgba(75, 192, 192, 1)',
                        backgroundColor: 'rgba(75, 192, 192, 0.2)',
                        fill: true,
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: 'day'
                        },
                        title: {
                            display: true,
                            text: 'Date'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Count'
                        }
                    }
                }
            }
        });
    });
}

function loadSocialEngagementChart() {
    $.get('/api/dashboard/social-engagement-chart-data', function (data) {
        const ctx = document.getElementById('socialEngagementChart').getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data.map(item => item.campaignName),
                datasets: [
                    {
                        label: 'Impressions',
                        data: data.map(item => item.impressions),
                        backgroundColor: 'rgba(255, 159, 64, 0.8)',
                    },
                    {
                        label: 'Likes',
                        data: data.map(item => item.likes),
                        backgroundColor: 'rgba(153, 102, 255, 0.8)',
                    },
                    {
                        label: 'Shares',
                        data: data.map(item => item.shares),
                        backgroundColor: 'rgba(201, 203, 207, 0.8)',
                    },
                    {
                        label: 'Comments',
                        data: data.map(item => item.comments),
                        backgroundColor: 'rgba(75, 192, 192, 0.8)',
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        stacked: true,
                        title: {
                            display: true,
                            text: 'Campaign'
                        }
                    },
                    y: {
                        stacked: true,
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Count'
                        }
                    }
                }
            }
        });
    });
}
