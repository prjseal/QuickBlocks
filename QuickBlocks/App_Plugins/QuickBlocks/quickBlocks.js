(function () {
    'use strict';

    function QuickBlocksController($scope, $http, editorState, navigationService, $location) {

        $scope.submit = function () {

            var url = $scope.model.url

            $http.get("/umbraco/backoffice/api/quickblocksapi/build/?url=" + url).then(function (response) {
                console.log(response);
            });
            // apiUrl = Umbraco.Sys.ServerVariables["QuickBlocs"]["QuickBlocksApiUrl"];
            //
            // $http.post(apiUrl + "CreateNewMedia", JSON.stringify({ MediaId: parseInt($scope.mediaId), QueryString: $scope.model.queryString, OverwriteExisting: overwriteExisting }),
            //     {
            //         headers: {
            //             'Content-Type': 'application/json'
            //         }
            //     }).then(function (response) {
            //     navigationService.hideDialog();
            //
            //     if (editorState.current.id != response.data) {
            //         $location.path('media/media/edit/' + response.data);
            //     }
            //     else {
            //         window.location.reload(true);
            //     }
            //
            // }, function (response) {
            //     navigationService.hideDialog();
            // });

        };

        var vm = this;
        var apiUrl;
        var mediaUrl;

        function init() {

            //apiUrl = Umbraco.Sys.ServerVariables["QuickBlocs"]["QuickBlocksApiUrl"];

            $scope.model = {
                url: "",
            };

            // $http.get(apiUrl + 'GetImageProccessorOptions').then(function (response) {
            //     $scope.availableImageProcessorOptions = response.data;
            // });
            //
            // if (editorState.current.contentTypeAlias === "Image") {
            //     mediaUrl = editorState.current.mediaLink;
            //     vm.mediaUrl = mediaUrl;
            //     vm.previewMediaUrl = mediaUrl;
            //     vm.fileName = editorState.current.mediaLink.replace(/^.*[\\\/]/, '');
            // }
            //
            // vm.selectedProcessorChanged = selectedProcessorChanged;
            // vm.setQueryString = setQueryString;
            // vm.debounce = 0;
            // vm.angular = angular;
            // vm.showQueryString = true;

        }
        
        init();

    }

    angular.module('umbraco').controller('QuickBlocksController', QuickBlocksController);

})();