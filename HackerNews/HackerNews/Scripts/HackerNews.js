class HackerNews {
    constructor(identifier) {

        this.identifier = identifier;
        this.$hackerNews = $.connection.hackerNewsHub;
        this.$tableContent = $("#TopNewsBodyId");

        this.$hackerNews.client.updateTopHackerNews = (message) => {
            this.drawTable(JSON.parse(message));
        };

        $.connection.hub.start().done(() => {
            this.$hackerNews.server.retrieveData(this.identifier);
        });
    }

    drawTable(rows) {
        this.$tableContent.html("");

        for (var i = 0; i < rows.length; i++) {
            const row = JSON.parse(rows[i]);
            const htmlRow =
                `<tr>
                    <td>${i + 1}</td>
                    <td>${row.by}</td>
                    <td>${row.title}</td>
                    <td><a href="${row.url}">History</a></td>
                </tr>`;

            this.$tableContent.append(htmlRow);
        }
    }
}