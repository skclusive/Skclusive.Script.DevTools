import resolve from "rollup-plugin-node-resolve";

process.env.INCLUDE_DEPS === "true";
module.exports = {
  input: "Redux/ReduxTool.js",
  output: {
    file: "wwwroot/ReduxTool.js",
    format: "iife"
  },
  plugins: [resolve()]
};
