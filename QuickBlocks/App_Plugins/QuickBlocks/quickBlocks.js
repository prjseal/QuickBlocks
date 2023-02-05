(function () {
    'use strict';

    function QuickBlocksController($scope, $http, editorState, navigationService, $location, notificationsService) {

        $scope.submit = function () {
            apiUrl = Umbraco.Sys.ServerVariables["QuickBlocks"]["QuickBlocksApi"];
           
            $http.post(apiUrl, JSON.stringify({ Url: $scope.model.url, HtmlBody: $scope.model.htmlbody }),
                {
                    headers: {
                        'Content-Type': 'application/json'
                    }
                }).then(function (response) {
                console.log(response);
                notificationsService.success('QuickBlocks', 'Your Block List has been created successfully');

            }, function (response) {
                console.log('error');
                notificationsService.success('QuickBlocks', 'There was an error when trying to process your request. Check the console for more details.');
            });

        };

        var vm = this;
        var apiUrl;

        vm.changeTab = changeTab;

        vm.tabs = [
            {
                "alias": "htmlSnippet",
                "label": "HTML Snippet",
                "active": true
            },
            {
                "alias": "fetchUrl",
                "label": "Fetch URL"
            }
        ];

        function changeTab(selectedTab) {
            vm.tabs.forEach(function(tab) {
                tab.active = false;
            });
            selectedTab.active = true;
        };
        
        function init() {

            apiUrl = Umbraco.Sys.ServerVariables["QuickBlocks"]["QuickBlocksApi"];

            $scope.model = {
                url: '',
                htmlbody: ''
            };

            vm.htmlEditorOptions = {
                autoFocus: false,
                showGutter: true,
                useWrapMode: true,
                showInvisibles: false,
                showIndentGuides: false,
                useSoftTabs: true,
                showPrintMargin: false,
                disableSearch: false,
                theme: "chrome",
                mode: "javascript",
                firstLineNumber: 1,
                advanced: {
                    fontSize: "small",
                    enableSnippets: false,
                    enableBasicAutocompletion: false,
                    enableLiveAutocompletion: false,
                    minLines: undefined,
                    maxLines: undefined,
                    wrap: true
                },
            };
        }
        
        init();

    }

    angular.module('umbraco').controller('QuickBlocksController', QuickBlocksController);

})();