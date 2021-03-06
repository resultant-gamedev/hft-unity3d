const webpack = require('webpack');
const path = require('path');
var plugins = require('webpack-load-plugins')();

module.exports = {
    entry: './sample-ui.js',
    output: {
        path: path.join(__dirname, '../Assets/WebPlayerTemplates/HappyFunTimes/sample-ui'),
        filename: 'sample-ui-min.js',
    },
    plugins: [
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                side_effects: false,
            },
            output: {
                comments: false,
            },
        }),
    ],
}

