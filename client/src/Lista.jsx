import React from "react";
import ingredients from "./ingredients.json"; // Direktimport

export default function Lista() {
    
  console.log("Imported ingredients:", ingredients);

  return (
    <div>
      <h1>Ingrediens Lista</h1>
      <ul>
        {ingredients.length > 0 ? (
          ingredients.map((item) => (
            <li key={item.id}>
              {item.name} - <em>{item.category}</em>
            </li>
          ))
        ) : (
          <p>🔍 Ingen data hittades</p>
        )}
      </ul>
    </div>
  );
}
