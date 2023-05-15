(function () {
    'use strict';

    function QuickBlocksController($scope, $http, editorState, navigationService, $location, notificationsService) {        
        
        var vm = this;
        var apiUrl;

        vm.submitState = "init";
        vm.reportState = "init";
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
            },
            {
                "alias": "report",
                "label": "Report"
            }
        ];

        function changeTab(selectedTab) {
            vm.tabs.forEach(function(tab) {
                tab.active = false;
            });
            selectedTab.active = true;
        };

        $scope.submit = function () {
            vm.submitState ="busy";
            apiUrl = Umbraco.Sys.ServerVariables["QuickBlocks"]["QuickBlocksApi"];

            $http.post(apiUrl, JSON.stringify({ Url: $scope.model.url, HtmlBody: $scope.model.htmlbody }),
                {
                    headers: {
                        'Content-Type': 'application/json'
                    }
                }).then(function (response) {
                    $scope.model.report = response.data;
                    console.log(response.data);
                    notificationsService.success('QuickBlocks', 'Your Block List has been created successfully');
                    vm.submitState = "success";
                }, function (response) {
                    console.log('error');
                    notificationsService.success('QuickBlocks', 'There was an error when trying to process your request. Check the console for more details.');
                    vm.submitState = "error";
                });

        };

        $scope.report = function () {
            vm.reportState = "busy";
            apiUrl = Umbraco.Sys.ServerVariables["QuickBlocks"]["QuickBlocksApi"];

            $http.post(apiUrl, JSON.stringify({ Url: $scope.model.url, HtmlBody: $scope.model.htmlbody, ReadOnly: true }),
                {
                    headers: {
                        'Content-Type': 'application/json'
                    }
                }).then(function (response) {
                    $scope.model.report = response.data;
                    console.log(response.data);
                    notificationsService.success('QuickBlocks', 'Your report has been created successfully');
                    vm.reportState = "success";
                    changeTab(vm.tabs[2]);
                }, function (reportState) {
                    console.log('error');
                    notificationsService.success('QuickBlocks', 'There was an error when trying to process your request. Check the console for more details.');
                    vm.reportState = "error";
                });

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