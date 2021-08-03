<template>
  <v-app>
    <v-main>
      <v-responsive :aspect-ratio="16 / 9">
        <v-form v-if="computedSchema" v-model="valid">
          <v-jsf v-model="model" :schema="computedSchema" >
            <template slot="custom-string1" slot-scope="{value, label, on}"><p class="mt-4">
                {{label}} <input type="text" :value="value" v-on="on" style="border:1px solid green;">.</p>
              </template>
          </v-jsf>
        </v-form>
        <pre></pre>
      </v-responsive>
    </v-main>
  </v-app>
</template>

<script>
import VJsf from "@koumoul/vjsf/lib/VJsf.js";
import "@koumoul/vjsf/lib/VJsf.css";
import "@koumoul/vjsf/lib/deps/third-party.js";
import _ from "lodash";
const axios = require("axios");

export default {
  name: "App",
  components: { VJsf },
  data: () => ({
    valid: false,
    dataResponse: undefined,
    model: {},
  }),
  computed: {
    computedSchema: function () {
      if (!this.dataResponse) {
        return undefined;
      }
      var sections = _.uniq(
        _.map(this.dataResponse, (r) => {
          return r.section;
        })
      );
      let sectionar = [];
      for (let i = 0; i < sections.length; i++) {
        sectionar.push({
          title: sections[i],
          type: "object",
          properties: {},
        });
      }

      for (let i = 0; i < this.dataResponse.length; i++) {
        let field = this.dataResponse[i];
        var section = _.find(sectionar, (s) => {
          return s.title === field.section;
        });
        var isSlider= field.format === 'slider'
        section.properties[field.name] = {
          type: field.type,
          title: field.name,
          format: field.format,
          "x-display": isSlider ? 'slider' : undefined,
          "x-props": {
            outlined: true,
            shaped: true,
            label: field.name,
          }
        };
        if(isSlider) {
          section.properties[field.name]["x-step"] = 1
          section.properties[field.name]["x-props"]["min"] = 0
          section.properties[field.name]["x-props"]["max"] = field.max
          //section.properties[field.name]["x-props"]["thumb-label"] = "always"
        }
      }
      let schema = {
        type: "object",
        "x-display": "tabs",
        "x-props": {
          grow: true,
        },
        allOf: sectionar,
      };
      return schema;
    },
  },
  methods: {},
  created() {
    axios
      .get("http://localhost:53480/moto")
      .then((response) => {
        this.dataResponse = response.data;
      })
      .catch(function (error) {
        // handle error
        console.error(error);
      });
  },
};
</script>