
const base =document.location.origin

export default function withBasePath(path) {
console.log(base);
  return `${base}/${path}`

}
