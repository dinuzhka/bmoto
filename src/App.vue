<template>
  <v-app>
    <v-main>
      <v-responsive :aspect-ratio="16 / 9">
        <v-form v-if="computedSchema" v-model="valid" class="app-form">
          <v-jsf v-model="model" :schema="computedSchema" :options="options" >
            <template slot="custom-switch" slot-scope="slotProps">
              <!--<v-switch
                :model="value"
                flat
                :on="on"
                :label="`${label}`"
                :value="value"
              ></v-switch>-->
              {{label}}/{{slotProps}}
            </template>
            <template slot="custom-string1">
              <v-row>
                <v-col md="6">
                  <v-simple-table :dense="true">
                    <template v-slot:default>
                      <thead>
                        <tr>
                          <th>Mandatory Requirements</th>
                          <th colspan="2">
                            Points {{ totalMandatoryGiven }}/{{
                              totalMandatoryMax
                            }}
                          </th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr
                          v-for="mandatory in computedMandatoryPoints"
                          :key="mandatory.name"
                        >
                          <td>{{ mandatory.name }}</td>
                          <td>
                            <v-chip
                              :color="getColor(mandatory.given, mandatory.max)"
                              dark
                              small
                              >{{ mandatory.given }}</v-chip
                            >
                          </td>
                          <td>{{ mandatory.max }}</td>
                        </tr>
                      </tbody>
                    </template>
                  </v-simple-table>
                </v-col>
                <v-col md="6" v-if="pointSections">
                  <v-simple-table :dense="true">
                    <template v-slot:default>
                      <thead>
                        <tr>
                          <th>Type</th>
                          <th colspan="2">Points</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr
                          v-for="pointSection in pointSections"
                          :key="pointSection.name"
                        >
                          <td>{{ pointSection.name }}</td>
                          <td>
                            <v-chip
                              :color="
                                getColor(pointSection.given, pointSection.max)
                              "
                              dark
                              small
                              >{{ pointSection.given }}</v-chip
                            >
                          </td>
                          <td>{{ pointSection.max }}</td>
                        </tr>
                      </tbody>
                    </template>
                  </v-simple-table>
                  <v-row  class="mt-2">
                    <v-col md="4" class="font-weight-bold"
                      >Total points obtained</v-col
                    >
                    <v-col
                      ><v-chip
                        :color="getColor(totalGiven, totalMax)"
                        dark
                        small
                        >{{ totalGiven }}</v-chip
                      >
                      / {{ totalMax }}</v-col
                    >
                  </v-row>
                  <v-row>
                    <v-card
                      class="mx-auto"
                      :color="totalColor"
                      dark
                      max-width="400"
                    >
                      <v-card-text class="text-h5 font-weight-bold">
                        Driver is {{ finalRating }}
                      </v-card-text>
                    </v-card>
                    <v-btn
                      color="blue-grey"
                      class="ma-2 white--text"
                      @click="saveDataToDb"
                    >
                      Save to database
                      <v-icon right dark> mdi-cloud-upload </v-icon>
                    </v-btn>
                    <v-btn
                      :disabled="doingExport"
                      color="blue-grey"
                      class="ma-2 white--text"
                      @click="saveDataToDb"
                    >
                    <downloadExcel
                      class            = "btn"
                      :fetch           = "exportCsv"
                      :fields          = "json_fields"
                      :before-generate = "startDownload"
                      :before-finish   = "finishDownload"
                      >
                      Download Excel
                    </downloadExcel>
                    </v-btn>
                  </v-row>
                </v-col>
              </v-row>
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
const service_url = 'https://bmoto20210804183547x.azurewebsites.net/moto'
export default {
  name: "App",
  components: { VJsf },
  data: () => ({
    valid: false,
    dataResponse: undefined,
    model: {},
    options: {
      httpLib: axios
    },
    doingExport: false,
    json_fields: {
        'Complete name': 'name',
        'Date': 'date',
      },
  }),
  computed: {
    totalMandatoryMax: function () {
      return _.sumBy(this.computedMandatoryPoints, (p) => {
        return p.max;
      });
    },
    totalMax: function () {
      return _.sumBy(this.computedPoints, (p) => {
        return p.max;
      });
    },
    totalMandatoryGiven: function () {
      var sum = _.sumBy(this.computedMandatoryPoints, (p) => {
        return p.given;
      });
      return sum;
    },
    totalGiven: function () {
      var sum = _.sumBy(this.computedPoints, (p) => {
        return p.given;
      });
      return sum;
    },
    computedMandatoryPoints: function () {
      if (!this.dataResponse) {
        return undefined;
      }
      var mandatories = _.filter(this.dataResponse, (d) => {
        return d.mandatory
      });
      var mands = _.map(mandatories, (m) => {
        return {
          name: m.name,
          max: m.max,
          given: this.model[m.name],
        };
      })
      
      var lights = _.filter(this.dataResponse, (d) => {
        return d.section === 'lights'
      });
      mands.push({
        name: 'lights',
        max: _.sumBy(lights, (m)=>{
          return m.max
        }),
        given:  _.sum(
          _.map(lights, (m) => {
            return this.model[m.name] || 0
          })
        )
      })
      return mands
    },
    computedPoints: function () {
      if (!this.dataResponse) {
        return undefined;
      }
      var mandatories = _.filter(this.dataResponse, (d) => {
        return d.isPoints;
      });
      return _.map(mandatories, (m) => {
        return {
          name: m.name,
          max: m.max,
          given: this.model[m.name],
        };
      });
    },
    pointSections: function () {
      if (!this.dataResponse) {
        return undefined;
      }
      var points = _.filter(this.dataResponse, (d) => {
        return d.isPoints;
      });
      var pointSections = _.uniq(
        _.map(points, (p) => {
          return p.section;
        })
      );
      let sectionAr = [];
      for (let i = 0; i < pointSections.length; i++) {
        let sectionStr = pointSections[i];
        let matches = _.filter(points, (p) => {
          return p.section === sectionStr;
        });
        var total = _.sumBy(matches, (m) => {
          return m.max;
        });
        var given = _.sum(
          _.map(matches, (m) => {
            return this.model[m.name] || 0
          })
        );
        sectionAr.push({
          name: sectionStr,
          max: total,
          given: given,
        });
      }
      return sectionAr;
    },
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
        var isSlider = field.format === "slider";
        var isSwitch = field.format === "switch";
        var isAuto = field.format === "auto";
        var isSpecialFormat = ["slider", "textarea", 'switch'].indexOf(field.format) >= 0;
        section.properties[field.name] = {
          type: field.type,
          title: field.name,
          format: field.format,
          "x-display": isSpecialFormat ? field.format : undefined,
          "x-props": {
            outlined: true,
            shaped: true,
            label: field.name,
          },
        };
        if (isSlider) {
          section.properties[field.name]["x-step"] = 1;
          section.properties[field.name]["x-props"]["min"] = 0;
          section.properties[field.name]["x-props"]["max"] = field.max;
          section.properties[field.name]["x-props"]["thumb-label"] = "always";
          section.properties[field.name]["x-props"]["thumb-size"] = "24";
        } else if(isSwitch) {
          section.properties[field.name]["x-props"]["value"] = field.max;
          section.properties[field.name]["x-cols"] = 4;
        }
        if(isAuto) {
          // section.properties[field.name]["x-fromUrl"] = `${service_url}/fieldvalues/${field.name}`;
        }
      }

      sectionar.push({
        title: "SUMMARY",
        type: "object",
        properties: {
          "overall-summary": {
            type: "string",
            title: "overall-summary",
            "x-display": "custom-string1",
          },
        },
      });

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
    finalRating() {
      if (this.totalGiven >= 80 && this.totalMandatoryGiven >= 35) {
        return "good";
      } else if (this.totalGiven >= 60 && this.totalMandatoryGiven >= 30) {
        return "fair";
      } else if (this.totalGiven >= 40 && this.totalMandatoryGiven >= 25) {
        return "to be upgraded";
      } else {
        return "rejected";
      }
    },
    totalColor() {
      var rating = this.finalRating;
      if (rating === "good") {
        return "green";
      } else if (rating === "fair") {
        return "yellow";
      } else if (rating === "to be upgraded") {
        return "amber";
      } else {
        return "red";
      }
    },
  },
  methods: {
    getColor(given, max) {
      var percentage = (given * 100) / max;
      if (percentage < 40) return "red";
      else if (percentage < 60) return "yellow";
      else if (percentage < 80) return "amber";
      else return "green";
    },
    exportCsv: async function () {
      const response = await axios.get(`${service_url}/export`)
      var csv = response.data
      this.json_fields = {}
      if(csv.length > 0) {
        var first = csv[0]
        for(let key in first){
          this.json_fields[key] = key
        }
      }
      return csv
    },
    saveDataToDb: function () {
      var request = this.model
      for (let i = 0; i < this.dataResponse.length; i++) {
        var elem = this.dataResponse[i]
        if(elem.isPoints) {
          request[elem.name] = isNaN(request[elem.name]) ? 0 : parseFloat(request[elem.name])
        } else if(request[elem.name]) {
          request[elem.name] = request[elem.name].toLowerCase()
        } else {
          request[elem.name] = ''
        }
      }
      axios
      .post(`${service_url}`, {
        model: JSON.stringify(request)
      })
      .then((response) => {
        console.log(response)
        window.location.reload()
      })
      .catch(function (error) {
        console.log(error)
        window.location.reload()
      });
    },
    startDownload(){
      this.doingExport = true
    },
    finishDownload(){
      this.doingExport = false
    }
  },
  created() {
    axios
      .get(`${service_url}`)
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
<style scoped>
.app-form >>> .v-window__container {
  padding-top: 30px;
}
</style>