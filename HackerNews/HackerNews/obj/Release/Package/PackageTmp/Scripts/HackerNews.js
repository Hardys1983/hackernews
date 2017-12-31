class HackerNews {
    constructor() {
        this.$hackerNews = $.connection.hackerNewsHub;
        this.$tableContent = $("#topNewsBodyId");
        this.$timeContent = $("#timeId");
        this.$searchButton = $("#searchButtonId");
        this.$searchText = $("#searchTextId");

        this.$hackerNews.client.updateTopHackerNews = (message) => {
            const date = new Date().toLocaleString();

            this.$timeContent.html(date);
            this.drawTable(JSON.parse(message));
        };

        $.connection.hub.start().done(() => {
            this.$hackerNews.server.retrieveData();
        });

        this.$searchButton.on("click", () => {
            this.$hackerNews.server.filterBy(this.$searchText.val());
        });
    }

    drawTable(rows) {
        this.$tableContent.html("");

        for (var i = 0; i < rows.length; i++) {
            const htmlRow =
                `<tr>
                    <td>${i + 1}</td>
                    <td>${rows[i].by}</td>
                    <td>${rows[i].title}</td>
                    <td><a href="${rows[i].url}">History</a></td>
                </tr>`;

            this.$tableContent.append(htmlRow);
        }
    }
}