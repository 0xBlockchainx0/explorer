export const expressionExplainer = {
  robot: 'You start the robot dance!',
  wave: 'You start waving',
  fistpump: 'You start fist-pumping',
  tik: "Those movements are sexy!",
  hammer: "You know how to show off at the dance floor!",
  tektonik: "What a great dancer!",
  dontsee: "",
  handsair: "",
  shrug: "",
  disco: "",
  dab: "",
  headexplode: ""
}
export const validExpressions = Object.keys(expressionExplainer)

export type validExpression = keyof typeof validExpressions

export function isValidExpression(expression: any): expression is keyof typeof expressionExplainer {
  return validExpressions.includes(expression)
}
