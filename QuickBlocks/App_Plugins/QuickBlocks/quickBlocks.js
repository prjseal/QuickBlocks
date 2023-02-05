(function () {
    'use strict';

    function QuickBlocksController($scope, $http, editorState, navigationService, $location) {

        $scope.submit = function () {
            //apiUrl = Umbraco.Sys.ServerVariables["QuickBlocks"]["QuickBlocksApiUrl"];

            apiUrl = '/umbraco/backoffice/api/quickblocksapi/';
            
            $http.post(apiUrl + "build", JSON.stringify({ Url: $scope.model.url, HtmlBody: $scope.model.htmlbody }),
                {
                    headers: {
                        'Content-Type': 'application/json'
                    }
                }).then(function (response) {
                console.log(response);

            }, function (response) {
                console.log('error');
            });

        };

        var vm = this;
        var apiUrl;

        vm.changeTab = changeTab;

        vm.tabs = [
            {
                "alias": "codeSnippet",
                "label": "Code Snippet",
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

            //apiUrl = Umbraco.Sys.ServerVariables["QuickBlocs"]["QuickBlocksApiUrl"];

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